using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class SpringRestTemplateApiGenerator : EndpointsGeneratorBase<JpaConfig>
{
    private readonly ILogger<SpringRestTemplateApiGenerator> _logger;

    public SpringRestTemplateApiGenerator(ILogger<SpringRestTemplateApiGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "SpringRestTemplateGen";

    protected static string GetClassName(string fileName)
    {
        return $"Abstract{fileName.ToPascalCase()}Client";
    }

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Client;
    }

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Path.Combine(Config.GetApiPath(file, tag), $"{GetClassName(file.Options.Endpoints.FileName)}.java");
    }

    protected virtual List<string> GetMethodParams(Endpoint endpoint, bool withType = true, bool withBody = true)
    {
        var methodParams = new List<string>();
        foreach (var param in endpoint.GetRouteParams())
        {
            if (withType)
            {
                methodParams.Add($"{Config.GetType(param)} {param.GetParamName()}");
            }
            else
            {
                methodParams.Add(param.GetParamName());
            }
        }

        foreach (var param in endpoint.GetQueryParams())
        {
            if (withType)
            {
                methodParams.Add($"{Config.GetType(param)} {param.GetParamName()}");
            }
            else
            {
                methodParams.Add(param.GetParamName());
            }
        }

        var bodyParam = endpoint.GetJsonBodyParam();
        if (bodyParam != null && withBody)
        {
            if (withType)
            {
                methodParams.Add($"{Config.GetType(bodyParam)} {bodyParam.GetParamName()}");
            }
            else
            {
                methodParams.Add(bodyParam.GetParamName());
            }
        }

        return methodParams;
    }

    protected virtual IEnumerable<string> GetTypeImports(IEnumerable<Endpoint> endpoints, string tag)
    {
        var properties = endpoints.SelectMany(endpoint => endpoint.Params).Concat(endpoints.Where(endpoint => endpoint.Returns is not null).Select(endpoint => endpoint.Returns));
        return properties.SelectMany(property => property!.GetTypeImports(Config, tag));
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        var className = GetClassName(fileName);
        var packageName = Config.GetPackageName(endpoints.First(), tag);
        using var fw = new JavaWriter(filePath, _logger, packageName, null);

        WriteImports(endpoints, fw, tag);
        fw.WriteLine();

        var javaxOrJakarta = Config.PersistenceMode.ToString().ToLower();
        if (Config.GeneratedHint)
        {
            fw.WriteLine(0, Config.GeneratedAnnotation);
        }

        fw.WriteLine($"public abstract class {className} {{");

        fw.WriteLine();

        fw.WriteLine(1, $"protected RestTemplate restTemplate;");
        fw.WriteLine(1, $"protected String host;");

        fw.WriteLine();
        fw.WriteDocStart(1, "Constructeur par paramètres");
        fw.WriteLine(1, " * @param restTemplate");
        fw.WriteLine(1, " * @param host");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"protected {className}(RestTemplate restTemplate, String host) {{");
        fw.WriteLine(2, $"this.restTemplate = restTemplate;");
        fw.WriteLine(2, $"this.host = host;");
        fw.WriteLine(1, $"}}");

        fw.WriteLine();
        fw.WriteDocStart(1, "Méthode de récupération des headers");
        fw.WriteLine(1, " * @return les headers à ajouter à la requête");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"protected abstract HttpHeaders getHeaders();");

        foreach (var endpoint in endpoints)
        {
            WriteEndpoint(fw, endpoint);
        }

        fw.WriteLine("}");
    }

    protected virtual void WriteEndpoint(JavaWriter fw, Endpoint endpoint)
    {
        fw.WriteLine();
        WriteUriBuilderMethod(fw, endpoint);
        fw.WriteLine();
        WriteEndpointCallMethod(fw, endpoint);
    }

    protected virtual void WriteEndpointCallMethod(JavaWriter fw, Endpoint endpoint)
    {
        fw.WriteDocStart(1, endpoint.Description);

        foreach (var param in endpoint.Params)
        {
            fw.WriteLine(1, $" * @param {param.GetParamName()} {param.Comment}");
        }

        if (endpoint.Returns != null)
        {
            fw.WriteLine(1, $" * @return {endpoint.Returns.Comment}");
        }

        fw.WriteLine(1, " */");
        var returnType = "ResponseEntity";
        var returnClass = "(Class<?>) null";
        if (endpoint.Returns != null)
        {
            if (Config.GetType(endpoint.Returns) == "ResponseEntity" && Config.GetType(endpoint.Returns).Split('<').Length > 1)
            {
                returnType = $"ResponseEntity<{Config.GetType(endpoint.Returns).Split('<')[1].Split('>').First()}>";
                returnClass = $"{Config.GetType(endpoint.Returns).Split('<')[1].Split('>').First()}.class";
            }
            else if (Config.GetType(endpoint.Returns).Contains('<'))
            {
                returnType = $"ResponseEntity<{Config.GetType(endpoint.Returns)}>";
                returnClass = @$"new ParameterizedTypeReference<{Config.GetType(endpoint.Returns)}>() {{}}";
                fw.AddImport("org.springframework.core.ParameterizedTypeReference");
            }
            else
            {
                returnType = $"ResponseEntity<{Config.GetType(endpoint.Returns)}>";
                returnClass = $"{Config.GetType(endpoint.Returns)}.class";
            }
        }

        fw.WriteLine(1, $"public {returnType} {endpoint.NameCamel}({string.Join(", ", GetMethodParams(endpoint))}){{");
        fw.WriteLine(2, $"HttpHeaders headers = this.getHeaders();");
        fw.WriteLine(2, $"UriComponentsBuilder uri = this.{endpoint.NameCamel}UriComponentsBuilder({string.Join(", ", GetMethodParams(endpoint, false, false))});");
        var body = $"new HttpEntity<>({(endpoint.GetJsonBodyParam()?.GetParamName() != null ? $"{endpoint.GetJsonBodyParam()?.GetParamName()}, " : string.Empty)}headers)";
        if (endpoint.Returns != null)
        {
            fw.WriteLine(2, $"return this.restTemplate.exchange(uri.build().toUri(), HttpMethod.{endpoint.Method}, {body}, {returnClass});");
        }
        else
        {
            fw.WriteLine(2, $"return this.restTemplate.exchange(uri.build().toUri(), HttpMethod.{endpoint.Method}, {body}, {returnClass});");
        }

        fw.WriteLine(1, "}");
    }

    protected virtual void WriteImports(IEnumerable<Endpoint> endpoints, JavaWriter fw, string tag)
    {
        var imports = new List<string>();
        imports.AddRange(GetTypeImports(endpoints, tag).Distinct());
        imports.Add(Config.PersistenceMode.ToString().ToLower() + ".annotation.Generated");
        imports.Add("org.springframework.web.util.UriComponentsBuilder");
        imports.Add("org.springframework.web.client.RestTemplate");
        imports.Add("java.net.URI");
        imports.Add("org.springframework.http.HttpMethod");
        imports.Add("org.springframework.http.HttpEntity");
        imports.Add("org.springframework.http.HttpHeaders");
        imports.Add("org.springframework.http.ResponseEntity");
        fw.AddImports(imports);
    }

    protected virtual void WriteUriBuilderMethod(JavaWriter fw, Endpoint endpoint)
    {
        fw.WriteDocStart(1, $"UriComponentsBuilder pour la méthode {endpoint.NameCamel}");

        foreach (var param in endpoint.GetRouteParams().Concat(endpoint.GetQueryParams()))
        {
            fw.WriteLine(1, $" * @param {param.GetParamName()} {param.Comment}");
        }

        if (endpoint.Returns != null)
        {
            fw.WriteLine(1, $" * @return uriBuilder avec les query params remplis");
        }

        fw.WriteLine(1, " */");
        var returnType = "UriComponentsBuilder";
        var methodParams = GetMethodParams(endpoint, true, false);

        fw.WriteLine(1, $"protected {returnType} {endpoint.NameCamel}UriComponentsBuilder({string.Join(", ", methodParams)}) {{");
        var fullRoute = endpoint.FullRoute;
        fullRoute = "/" + fullRoute;
        foreach (IProperty p in endpoint.GetRouteParams())
        {
            fullRoute = fullRoute.Replace(@$"{{{p.GetParamName()}}}", "%s");
        }

        if (endpoint.GetRouteParams().Any())
        {
            fullRoute = $@"""{fullRoute}"".formatted({string.Join(", ", endpoint.GetRouteParams().Select(p => p.GetParamName()))})";
        }
        else
        {
            fullRoute = $@"""{fullRoute}""";
        }

        fw.WriteLine(2, @$"String uri = host + {fullRoute};");
        if (!endpoint.GetQueryParams().Any())
        {
            fw.WriteLine(2, @$"return UriComponentsBuilder.fromUri(URI.create(uri));");
            fw.WriteLine(1, "}");
            return;
        }

        fw.WriteLine(2, @$"UriComponentsBuilder uriBuilder = UriComponentsBuilder.fromUri(URI.create(uri));");
        foreach (IProperty p in endpoint.GetQueryParams())
        {
            var indentLevel = 2;
            if (!p.Required)
            {
                fw.WriteLine(2, @$"if ({p.GetParamName()} != null) {{");
                indentLevel++;
            }

            if (Config.GetType(p).StartsWith("List"))
            {
                fw.AddImport("java.util.stream.Collectors");
                fw.WriteLine(indentLevel, @$"uriBuilder.queryParam(""{p.GetParamName()}"", {p.GetParamName()}.stream().collect(Collectors.joining("","")));");
            }
            else
            {
                fw.WriteLine(indentLevel, @$"uriBuilder.queryParam(""{p.GetParamName()}"", {p.GetParamName()});");
            }

            if (!p.Required)
            {
                fw.WriteLine(2, @$"}}");
                fw.WriteLine();
            }
        }

        fw.WriteLine(2, $"return uriBuilder;");
        fw.WriteLine(1, "}");
    }
}