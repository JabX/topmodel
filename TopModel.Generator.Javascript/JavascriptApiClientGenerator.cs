using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class JavascriptApiClientGenerator(ILogger<JavascriptApiClientGenerator> logger, IFileWriterProvider writerProvider)
    : EndpointsGeneratorBase<JavascriptConfig>(logger, writerProvider)
{
    public override string Name => "JSApiClientGen";

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Config.GetEndpointsFileName(file, tag);
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        var fetch = Config.FetchPath != "@focus4/core" ? "fetch" : "coreFetch";
        var fetchImport = Config.FetchPath.StartsWith('@') || !Config.FetchPath.StartsWith('.')
            ? Config.FetchPath
            : Path.GetRelativePath(string.Join('/', filePath.Split('/').SkipLast(1)), Path.Combine(Config.OutputDirectory, Config.ResolveVariables(Config.FetchPath, tag))).Replace("\\", "/");

        using var fw = OpenFileWriter(filePath, false);

        fw.WriteLine($@"import {{{fetch}}} from ""{fetchImport}"";");

        var imports = Config.GetEndpointImports(filePath, endpoints, tag, Classes);
        if (imports.Any())
        {
            fw.WriteLine();

            foreach (var (import, path) in imports)
            {
                fw.WriteLine($@"import {{{import}}} from ""{path}"";");
            }
        }

        foreach (var endpoint in endpoints)
        {
            fw.WriteLine();
            fw.WriteLine("/**");
            fw.WriteLine($" * {endpoint.Description}");

            foreach (var param in endpoint.Params)
            {
                fw.WriteLine($" * @param {param.GetParamName()} {param.Comment}");
            }

            fw.WriteLine(" * @param options Options pour 'fetch'.");

            if (endpoint.Returns != null)
            {
                fw.WriteLine($" * @returns {endpoint.Returns.Comment}");
            }

            fw.WriteLine(" */");
            fw.Write($"export function {endpoint.NameCamel}(");

            foreach (var param in endpoint.Params)
            {
                var defaultValue = Config.GetValue(param, Classes);
                fw.Write($"{param.GetParamName()}{(param.IsQueryParam() && !endpoint.IsMultipart && defaultValue == "undefined" ? "?" : string.Empty)}: {Config.GetType(param, Classes)}{(defaultValue != "undefined" ? $" = {defaultValue}" : string.Empty)}, ");
            }

            fw.Write("options: RequestInit = {}): Promise<");
            if (endpoint.Returns == null)
            {
                fw.Write("void");
            }
            else
            {
                fw.Write(Config.GetType(endpoint.Returns, Classes));
            }

            fw.WriteLine("> {");

            if (endpoint.IsMultipart)
            {
                fw.WriteLine(1, "const body = new FormData();");
                fw.WriteLine(1, "fillFormData(");
                fw.WriteLine(2, "{");

                foreach (var param in endpoint.Params.Where(p => !p.IsRouteParam() && !p.IsQueryParam()))
                {
                    if (param is not CompositionProperty and not AliasProperty { Property: CompositionProperty })
                    {
                        fw.Write(3, $@"{param.GetParamName()}");
                    }
                    else
                    {
                        fw.Write(3, $@"...{param.GetParamName()}");
                    }

                    if (endpoint.Params.IndexOf(param) < endpoint.Params.Count - 1)
                    {
                        fw.WriteLine(",");
                    }
                    else
                    {
                        fw.WriteLine();
                    }
                }

                fw.WriteLine(2, "},");
                fw.WriteLine(2, "body");
                fw.WriteLine(1, ");");
            }

            fw.Write(1, $@"return {fetch}(""{endpoint.Method}"", `./{endpoint.FullRoute.Replace("{", "${")}`, {{");

            if (endpoint.GetJsonBodyParam() != null)
            {
                fw.Write($"body: {endpoint.GetJsonBodyParam()!.GetParamName()}");
            }
            else if (endpoint.IsMultipart)
            {
                fw.Write("body");
            }

            if ((endpoint.GetJsonBodyParam() != null || endpoint.IsMultipart) && endpoint.GetQueryParams().Any())
            {
                fw.Write(", ");
            }

            if (endpoint.GetQueryParams().Any())
            {
                fw.Write("query: {");

                foreach (var qParam in endpoint.GetQueryParams())
                {
                    fw.Write(qParam.GetParamName());

                    if (qParam != endpoint.GetQueryParams().Last())
                    {
                        fw.Write(", ");
                    }
                }

                fw.Write("}");
            }

            fw.WriteLine("}, options);");
            fw.WriteLine("}");
        }

        if (endpoints.Any(endpoint => endpoint.Params.Any(p => p is not CompositionProperty and not AliasProperty { Property: CompositionProperty } && Config.GetType(p).Contains("File"))))
        {
            fw.WriteLine(@"
function fillFormData(data: any, formData: FormData, prefix = """") {
    if (Array.isArray(data)) {
        data.forEach((item, i) => fillFormData(item, formData, prefix + (typeof item === ""object"" && !(item instanceof File) ? `[${i}]` : """")));
    } else if (typeof data === ""object"" && !(data instanceof File)) {
        for (const key in data) {
            fillFormData(data[key], formData, (prefix ? `${prefix}.` : """") + key);
        }
    } else {
        formData.append(prefix, data);
    }
}");
        }
    }
}