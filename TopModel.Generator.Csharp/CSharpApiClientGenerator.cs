using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Csharp;

public class CSharpApiClientGenerator(ILogger<CSharpApiClientGenerator> logger, GeneratedFileWriterProvider writerProvider)
    : EndpointsGeneratorBase<CsharpConfig>(logger, writerProvider)
{
    public override string Name => "CSharpApiClientGen";

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Client;
    }

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Path.Combine(Config.GetApiPath(file, tag), "generated", $"{file.Options.Endpoints.FileName.ToPascalCase()}Client.cs");
    }

    protected virtual string GetNamespace(Class classe, string tag)
    {
        return Config.GetNamespace(classe, classe.Tags.Contains(tag) ? tag : classe.Tags.Intersect(Config.Tags).FirstOrDefault() ?? tag);
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        var className = $"{fileName.ToPascalCase()}Client";
        var ns = Config.GetNamespace(endpoints.First(), tag);

        HandleFilePartial(filePath.Replace($"{Path.DirectorySeparatorChar}generated", string.Empty).Replace(".cs", ".partial.cs"), className, ns);

        using var fw = this.OpenCSharpWriter(filePath);

        var hasBody = endpoints.Any(e => e.GetJsonBodyParam() != null);
        var hasReturn = endpoints.Any(e => e.Returns != null && !new[] { "string", "byte[]" }.Contains(Config.GetType(e.Returns)?.TrimEnd('?')));
        var hasJson = hasReturn || hasBody;

        var usings = new List<string>();

        if (endpoints.Any(e => e.Returns != null && !e.Returns.Required))
        {
            usings.Add("System.Net");
        }

        if (hasJson)
        {
            usings.Add("System.Net.Http.Json");
            usings.Add("System.Text.Json");
            usings.Add("System.Text.Json.Serialization");
        }

        if (endpoints.Any(e => e.GetQueryParams().Any()))
        {
            if (endpoints.Any(e => e.GetQueryParams().Any(qp =>
            {
                var typeName = Config.GetType(qp);
                return !typeName.StartsWith("string") && !typeName.StartsWith("Guid");
            })))
            {
                usings.Add("System.Globalization");
            }
        }

        foreach (var property in endpoints.SelectMany(e => e.Properties))
        {
            usings.AddRange(Config.GetDomainImports(property, tag));

            if (property.IsQueryParam())
            {
                usings.AddRange(Config.GetValueImports(property));
            }

            switch (property)
            {
                case AssociationProperty ap when Config.CanClassUseEnums(ap.Association, Classes):
                    usings.Add(GetNamespace(ap.Association, tag));
                    break;
                case AliasProperty { Property: AssociationProperty ap2 } when Config.CanClassUseEnums(ap2.Association, Classes):
                    usings.Add(GetNamespace(ap2.Association, tag));
                    break;
                case AliasProperty { PrimaryKey: false, Property: RegularProperty { PrimaryKey: true } rp } when Config.CanClassUseEnums(rp.Class, Classes):
                    usings.Add(GetNamespace(rp.Class, tag));
                    break;
                case CompositionProperty cp:
                    usings.Add(GetNamespace(cp.Composition, tag));
                    break;
                case AliasProperty { Property: CompositionProperty cp }:
                    usings.Add(GetNamespace(cp.Composition, tag));
                    break;
            }
        }

        fw.WriteUsings(usings.Distinct().Where(u => u != ns).ToArray());
        if (usings.Any())
        {
            fw.WriteLine();
        }

        fw.WriteNamespace(ns);

        var parameters = "HttpClient client";

        fw.WriteSummary($"Client {fileName}");
        if (Config.UsePrimaryConstructors)
        {
            fw.WriteParam("client", "HttpClient injecté.", 0);
        }

        fw.WriteClassDeclaration(className, null, false, parameters: Config.UsePrimaryConstructors ? parameters : null);

        if (!Config.UsePrimaryConstructors)
        {
            fw.WriteLine(1, "private readonly HttpClient _client;");
        }

        if (hasJson)
        {
            fw.WriteLine(1, "private readonly JsonSerializerOptions _jsOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };");
        }

        if (!Config.UsePrimaryConstructors)
        {
            fw.WriteLine();
            fw.WriteSummary(1, "Constructeur");
            fw.WriteParam("client", "HttpClient injecté.");
            fw.WriteLine(1, $"public {className}(HttpClient client)");
            fw.WriteLine(1, "{");
            fw.WriteLine(2, "_client = client;");
            fw.WriteLine(1, "}");
        }

        var orderedEndpoints = endpoints.OrderBy(endpoint => endpoint.NamePascal).ToList();

        foreach (var endpoint in orderedEndpoints)
        {
            if (orderedEndpoints.IndexOf(endpoint) > 0 || !Config.UsePrimaryConstructors || hasJson)
            {
                fw.WriteLine();
            }

            fw.WriteSummary(1, endpoint.Description);

            foreach (var param in endpoint.Params)
            {
                fw.WriteParam(param.GetParamName(), param.Comment);
            }

            fw.WriteReturns(1, endpoint.Returns?.Comment ?? "Task.");

            fw.Write("    public async Task");

            var returnType = endpoint.Returns != null ? Config.GetType(endpoint.Returns, nonNullable: endpoint.Returns.Required) : null;
            if (returnType?.StartsWith("IAsyncEnumerable") ?? false)
            {
                returnType = returnType.Replace("IAsyncEnumerable", "IEnumerable");
            }

            if (returnType != null)
            {
                fw.Write($"<{returnType}>");
            }

            fw.Write($" {endpoint.NamePascal}(");

            foreach (var param in endpoint.Params)
            {
                fw.Write($"{Config.GetType(param, nonNullable: param.IsJsonBodyParam() || param.IsRouteParam() || param.IsQueryParam() && Config.GetValue(param, Classes) != "null")} {param.GetParamName().Verbatim()}");

                if (param.IsQueryParam())
                {
                    fw.Write($" = {Config.GetValue(param, Classes)}");
                }

                if (endpoint.Params.Last() != param)
                {
                    fw.Write(", ");
                }
            }

            fw.WriteLine(")");
            fw.WriteLine(1, "{");

            var bodyParam = endpoint.GetJsonBodyParam();

            fw.WriteLine(2, $"await EnsureAuthentication();");

            if (endpoint.GetQueryParams().Any())
            {
                fw.WriteLine(2, "var query = await new FormUrlEncodedContent(new Dictionary<string, string>");
                fw.WriteLine(2, "{");

                foreach (var qp in endpoint.GetQueryParams().Where(qp => !Config.GetType(qp).Contains("[]")))
                {
                    var toString = Config.GetType(qp)?.TrimEnd('?') switch
                    {
                        "string" => string.Empty,
                        "Guid" => "?.ToString()",
                        _ => $"?.ToString(CultureInfo.InvariantCulture)"
                    };

                    fw.WriteLine(3, $@"[""{qp.GetParamName()}""] = {qp.GetParamName().Verbatim()}{toString},");
                }

                var listQPs = endpoint.GetQueryParams().Where(qp => Config.GetType(qp).Contains("[]")).ToList();

                if (listQPs.Count == 0)
                {
                    fw.WriteLine(2, "}.Where(kv => kv.Value != null)).ReadAsStringAsync();");
                }
                else
                {
                    fw.Write("        }");
                    foreach (var qp in listQPs)
                    {
                        var toString = Config.GetType(qp) switch
                        {
                            "string[]" => string.Empty,
                            "Guid[]" => ".ToString()",
                            _ => $".ToString(CultureInfo.InvariantCulture)"
                        };

                        var first = listQPs.IndexOf(qp) == 0;
                        fw.WriteLine(first ? 0 : 3, $@"{(first ? string.Empty : " ")}.Concat({qp.GetParamName()}?.Select(i => new KeyValuePair<string, string>(""{qp.GetParamName()}"", i{toString})) ?? new Dictionary<string, string>())");
                    }

                    fw.WriteLine(2, " .Where(kv => kv.Value != null)).ReadAsStringAsync();");
                }
            }

            fw.WriteLine(2, $"using var res = await {(Config.UsePrimaryConstructors ? string.Empty : "_")}client.SendAsync(new(HttpMethod.{endpoint.Method.ToPascalCase(true)}, $\"{endpoint.FullRoute}{(endpoint.GetQueryParams().Any() ? "?{query}" : string.Empty)}\"){(bodyParam != null ? $" {{ Content = JsonContent.Create({bodyParam.NameCamel}, options: _jsOptions) }}" : string.Empty)}{(returnType != null ? ", HttpCompletionOption.ResponseHeadersRead" : string.Empty)});");
            fw.WriteLine(2, $"await EnsureSuccess(res);");

            if (returnType != null)
            {
                if (!endpoint.Returns!.Required)
                {
                    fw.WriteLine();
                    fw.WriteLine(2, "if (res.StatusCode == HttpStatusCode.NoContent)");
                    fw.WriteLine(2, "{");
                    fw.WriteLine(3, "return null;");
                    fw.WriteLine(2, "}");
                }

                if (returnType.TrimEnd('?') == "string")
                {
                    fw.WriteLine();
                    fw.WriteLine(2, $"return (await res.Content.ReadAsStringAsync()).Trim('\"');");
                }
                else if (returnType == "byte[]")
                {
                    fw.WriteLine();
                    fw.WriteLine(2, "using var ms = new MemoryStream();");
                    fw.WriteLine(2, "(await res.Content.ReadAsStreamAsync()).CopyTo(ms);");
                    fw.WriteLine(2, "return ms.ToArray();");
                }
                else
                {
                    fw.WriteLine();
                    fw.Write(2, $"return ");

                    if (Config.NullableEnable && endpoint.Returns.Required)
                    {
                        fw.Write("(");
                    }

                    fw.Write($"await res.Content.ReadFromJsonAsync<{returnType}>(_jsOptions)");

                    if (Config.NullableEnable && endpoint.Returns.Required)
                    {
                        fw.Write(")!");
                    }

                    fw.WriteLine(";");
                }
            }

            fw.WriteLine(1, "}");
        }

        fw.WriteLine();
        fw.WriteSummary(1, "Assure que l'authentification est configurée.");
        fw.WriteLine(1, "private partial Task EnsureAuthentication();");

        fw.WriteLine();
        fw.WriteSummary(1, "Gère les erreurs éventuelles retournées par l'API appelée.");
        fw.WriteParam("response", "Réponse HTTP");
        fw.WriteLine(1, "private partial Task EnsureSuccess(HttpResponseMessage response);");

        fw.WriteLine("}");
    }

    protected virtual void HandleFilePartial(string filePath, string className, string ns)
    {
        if (File.Exists(filePath))
        {
            return;
        }

        using var w = this.OpenCSharpWriter(filePath);
        w.EnableHeader = false;

        w.WriteNamespace(ns);

        w.WriteSummary($"Client {className[..^6]}");
        w.WriteClassDeclaration(className, null, false);

        w.WriteLine(1, "private partial Task EnsureAuthentication()");
        w.WriteLine(1, "{");
        w.WriteLine(2, "return Task.CompletedTask;");
        w.WriteLine(1, "}");

        w.WriteLine();
        w.WriteLine(1, "private partial Task EnsureSuccess(HttpResponseMessage response)");
        w.WriteLine(1, "{");
        w.WriteLine(2, "response.EnsureSuccessStatusCode();");
        w.WriteLine(2, "return Task.CompletedTask;");
        w.WriteLine(1, "}");

        w.WriteLine("}");
    }
}