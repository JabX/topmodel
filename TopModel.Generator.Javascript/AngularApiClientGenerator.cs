using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class AngularApiClientGenerator(ILogger<AngularApiClientGenerator> logger, GeneratedFileWriterProvider writerProvider)
    : EndpointsGeneratorBase<JavascriptConfig>(logger, writerProvider)
{
    public override string Name => "JSNGApiClientGen";

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Config.GetEndpointsFileName(file, tag);
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        using var fw = OpenFileWriter(filePath, false);
        var imports = Config.GetEndpointImports(filePath, endpoints, tag, Classes);

        imports.AddRange([
            (Import: "Injectable", Path: "@angular/core"),
            (Import: "HttpClient", Path: "@angular/common/http"),
        ]);

        if (Config.ApiMode == TargetFramework.ANGULAR)
        {
            imports.Add((Import: "Observable", Path: "rxjs"));
        }
        else
        {
            imports.Add((Import: "lastValueFrom", Path: "rxjs"));
        }

        if (endpoints.Any(e => e.GetQueryParams().Any()))
        {
            imports.Add((Import: "HttpParams", Path: "@angular/common/http"));
        }

        imports = imports.GroupAndSort();

        if (imports.Count > 0)
        {
            fw.WriteLine();

            foreach (var (import, path) in imports)
            {
                fw.WriteLine($@"import {{ {import} }} from ""{path}"";");
            }
        }

        fw.WriteLine("@Injectable({");
        fw.WriteLine(1, "providedIn: 'root'");
        fw.WriteLine("})");

        fw.WriteLine(@$"export class {fileName.ToPascalCase()}Service {{");
        fw.WriteLine();
        fw.WriteLine(1, "constructor(private readonly http: HttpClient) {}");
        foreach (var endpoint in endpoints)
        {
            WriteEndpoint(endpoint, fw);
        }

        if (endpoints.Any(e => e.IsMultipart))
        {
            fw.WriteLine(@"
    private fillFormData(data: any, formData: FormData, prefix = """") {
        if (Array.isArray(data)) {
            data.forEach((item, i) => this.fillFormData(item, formData, prefix + (typeof item === ""object"" && !(item instanceof File) ? `[${i}]` : """")));
        } else if (typeof data === ""object"" && !(data instanceof File)) {
            for (const key in data) {
                this.fillFormData(data[key], formData, (prefix ? `${prefix}.` : """") + key);
            }
        } else {
            formData.append(prefix, data);
        }
    }");
        }

        fw.WriteLine("}");
    }

    private void WriteEndpoint(Endpoint endpoint, GeneratedFileWriter fw)
    {
        fw.WriteLine();
        fw.WriteLine(1, "/**");
        fw.WriteLine(1, $" * @description {endpoint.Description}");
        var fullRoute = endpoint.FullRoute.Replace("{", "${");
        foreach (var param in endpoint.Params)
        {
            fw.WriteLine(1, $" * @param {param.GetParamName()} {param.Comment}");
        }

        if (endpoint.Returns != null)
        {
            fw.WriteLine(1, $" * @returns {endpoint.Returns.Comment}");
        }

        fw.WriteLine(1, " */");
        fw.Write(1, $"{endpoint.NameCamel}(");

        var hasProperty = false;
        foreach (var param in endpoint.Params)
        {
            if (hasProperty)
            {
                fw.Write(", ");
            }

            hasProperty = true;
            var defaultValue = Config.GetValue(param, Classes);
            fw.Write($"{param.GetParamName()}{(param.IsQueryParam() && !endpoint.IsMultipart && defaultValue == "undefined" ? "?" : string.Empty)}: {Config.GetType(param, Classes)}{(defaultValue != "undefined" ? $" = {defaultValue}" : string.Empty)}");
        }

        if (endpoint.GetQueryParams().Any())
        {
            if (hasProperty)
            {
                fw.Write(", ");
            }

            fw.Write("queryParams: any = {}");
        }

        if (Config.ApiMode == TargetFramework.ANGULAR)
        {
            fw.Write("): Observable<");
        }
        else
        {
            fw.Write("): Promise<");
        }

        string returnType;
        if (endpoint.Returns == null)
        {
            returnType = "void";
        }
        else
        {
            returnType = Config.GetType(endpoint.Returns, Classes);
        }

        fw.Write(returnType);
        fw.WriteLine("> {");

        if (endpoint.IsMultipart)
        {
            fw.WriteLine(2, "const body = new FormData();");
            fw.WriteLine(2, "this.fillFormData(");
            fw.WriteLine(3, "{");

            foreach (var param in endpoint.Params.Where(p => !p.IsRouteParam() && !p.IsQueryParam()))
            {
                if (param is not CompositionProperty and not AliasProperty { Property: CompositionProperty })
                {
                    fw.Write($@"                {param.GetParamName()}");
                }
                else
                {
                    fw.Write($@"                ...{param.GetParamName()}");
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

            fw.WriteLine(3, "},");
            fw.WriteLine(3, "body");
            fw.WriteLine(2, ");");
        }

        if (endpoint.GetQueryParams().Any())
        {
            foreach (var qParam in endpoint.GetQueryParams())
            {
                fw.WriteLine(2, @$"if ({qParam.GetParamName()} !== null && {qParam.GetParamName()} !== undefined) {{");
                fw.WriteLine(3, $"queryParams['{qParam.GetParamName()}'] = {qParam.GetParamName()}");
                fw.WriteLine(2, @$"}}");
                fw.WriteLine();
            }

            fw.WriteLine(2, "const httpParams = new HttpParams({fromObject: queryParams});");
            fw.WriteLine(2, "const httpOptions = {params: httpParams}");

            fw.WriteLine();
        }

        var needResponseType = returnType == "string" || returnType == "Blob" || returnType == "ArrayBuffer";
        var getter = $"{endpoint.Method.ToLower()}<{returnType}>";
        if (needResponseType)
        {
            getter = $"{endpoint.Method.ToLower()}";
        }

        fw.Write(2, $@"return {(Config.ApiMode == TargetFramework.ANGULAR_PROMISE ? "lastValueFrom(" : string.Empty)}this.http.{getter}(`/{fullRoute}`");
        if (endpoint.GetJsonBodyParam() != null)
        {
            fw.Write($", {endpoint.GetJsonBodyParam()!.GetParamName()}");
        }
        else if (endpoint.IsMultipart)
        {
            fw.Write($", body");
        }
        else if (endpoint.Method != "OPTIONS" && endpoint.Method != "GET" && endpoint.Method != "DELETE")
        {
            fw.Write(", {}");
        }

        if (needResponseType)
        {
            var responseType = returnType == "string" ? "text" : returnType.ToLower();
            fw.Write($", {{responseType: '{responseType}'}}");
        }

        if (endpoint.GetQueryParams().Any())
        {
            fw.Write(", httpOptions");
        }

        fw.WriteLine($"{(Config.ApiMode == TargetFramework.ANGULAR_PROMISE ? ")" : string.Empty)});");
        fw.WriteLine(1, "}");
    }
}