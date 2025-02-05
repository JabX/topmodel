using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class NuxtApiClientGenerator(ILogger<NuxtApiClientGenerator> logger, IFileWriterProvider writerProvider)
    : EndpointsGeneratorBase<JavascriptConfig>(logger, writerProvider)
{
    public override string Name => "JSApiClientGen";

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Config.GetEndpointsFileName(file, tag);
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        using var fw = OpenFileWriter(filePath, false);

        fw.WriteLine($@"import {{AsyncData, AsyncDataOptions}} from ""nuxt/app"";");

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

            var fetchReturnType = endpoint.Returns == null ? "void" : Config.GetType(endpoint.Returns, Classes);
            fw.WriteLine($"options: AsyncDataOptions<{fetchReturnType}> = {{}}): AsyncData<{fetchReturnType} | null, Error | null> {{");

            if (endpoint.IsMultipart)
            {
                fw.WriteLine("    const body = new FormData();");
                fw.WriteLine("    fillFormData(");
                fw.WriteLine("        {");

                foreach (var param in endpoint.Params.Where(p => !p.IsRouteParam() && !p.IsQueryParam()))
                {
                    if (param is not CompositionProperty and not AliasProperty { Property: CompositionProperty })
                    {
                        fw.Write($@"            {param.GetParamName()}");
                    }
                    else
                    {
                        fw.Write($@"            ...{param.GetParamName()}");
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

                fw.WriteLine("        },");
                fw.WriteLine("        body");
                fw.WriteLine("    );");
            }

            var fetchRoute = $@"`/{endpoint.FullRoute.Replace("{", "${")}`";

            fw.WriteLine(1, $@"return useAsyncData({fetchRoute}, () => ");
            fw.WriteLine(2, $@"$fetch<{fetchReturnType}>({fetchRoute}, {{");
            fw.WriteLine(3, $@"method: '{endpoint.Method}',");

            if (endpoint.IsMultipart)
            {
                fw.WriteLine(3, "body,");
            }
            else if (endpoint.GetJsonBodyParam() != null)
            {
                fw.WriteLine(3, $@"body: {endpoint.GetJsonBodyParam()!.GetParamName()},");
            }

            if (endpoint.GetQueryParams().Any())
            {
                fw.WriteLine(4, "query: {");

                foreach (var qParam in endpoint.GetQueryParams())
                {
                    fw.WriteLine(5, $@"{qParam.GetParamName()},");
                }

                fw.WriteLine(4, "}");
            }

            fw.WriteLine(2, "}), options);");
            fw.WriteLine("}");
        }

        if (endpoints.Any(endpoint => endpoint.IsMultipart))
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
