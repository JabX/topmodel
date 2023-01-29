﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class SpringClientApiGenerator : GeneratorBase<object>
{
    private readonly JpaConfig _config;
    private readonly ILogger<SpringClientApiGenerator> _logger;

    public SpringClientApiGenerator(ILogger<SpringClientApiGenerator> logger, JpaConfig config)
        : base(logger, config)
    {
        _config = config;
        _logger = logger;
    }

    public override string Name => "SpringApiClientGen";

    public override IEnumerable<string> GeneratedFiles => Files.Values.Where(f => f.Endpoints.Any()).Select(f => GetFilePath(f.Options.Endpoints.FileName, f.Module)).Distinct();

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        foreach (var file in files.GroupBy(file => new { file.Options.Endpoints.FileName, file.Module }))
        {
            GenerateClient(file.Key.FileName, file.Key.Module);
        }
    }

    private string GetDestinationFolder(string module)
    {
        return Path.Combine(_config.OutputDirectory, Path.Combine(_config.ApiRootPath!.ToLower().Split(".")), Path.Combine(_config.ApiPackageName.Split('.')), Path.Combine(module.ToLower().Split(".")));
    }

    private string GetClassName(string fileName)
    {
        return $"Abstract{fileName.ToPascalCase()}Client";
    }

    private string GetFileName(string fileName)
    {
        return $"{GetClassName(fileName)}.java";
    }

    private string GetFilePath(string fileName, string module)
    {
        return Path.Combine(GetDestinationFolder(module), GetFileName(fileName));
    }

    private void GenerateClient(string fileName, string module)
    {
        var files = Files.Values
            .Where(file => file.Options.Endpoints.FileName == fileName && file.Module == module);

        var endpoints = files
            .SelectMany(file => file.Endpoints)
            .OrderBy(endpoint => endpoint.Name, StringComparer.Ordinal)
            .ToList();

        if (!endpoints.Any() || _config.ApiRootPath == null)
        {
            return;
        }

        foreach (var endpoint in endpoints)
        {
            CheckEndpoint(endpoint);
        }

        var destFolder = GetDestinationFolder(module);
        Directory.CreateDirectory(destFolder);
        var packageName = $"{_config.ApiPackageName}.{module.ToLower()}";
        using var fw = new JavaWriter($"{GetFilePath(fileName, module)}", _logger, packageName, null);

        WriteImports(files, fw);
        fw.WriteLine();

        fw.WriteLine("@Generated(\"TopModel : https://github.com/klee-contrib/topmodel\")");
        fw.WriteLine($"public abstract class {GetClassName(fileName)} {{");

        fw.WriteLine();

        fw.WriteLine(1, $"protected RestTemplate restTemplate;");
        fw.WriteLine(1, $"protected String host;");

        fw.WriteLine();
        fw.WriteDocStart(1, "Constructeur par paramètres");
        fw.WriteLine(1, " * @param restTemplate");
        fw.WriteLine(1, " * @param host");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"protected {GetClassName(fileName)}(RestTemplate restTemplate, String host) {{");
        fw.WriteLine(2, $"this.restTemplate = restTemplate;");
        fw.WriteLine(2, $"this.host = host;");
        fw.WriteLine(1, $"}}");

        GetClassName(fileName);
        fw.WriteLine();
        fw.WriteDocStart(1, "Méthode de récupération des headers");
        fw.WriteLine(1, " * @return les headers à ajouter à la requête");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"protected abstract HttpHeaders getHeaders();");
        foreach (var endpoint in endpoints)
        {
            WriteEndPoint(fw, endpoint);
        }

        fw.WriteLine("}");
    }

    private void WriteEndPoint(JavaWriter fw, Endpoint endpoint)
    {
        fw.WriteLine();
        WriteUriBuilderMethod(fw, endpoint);
        fw.WriteLine();
        WriteEndpointCallMethod(fw, endpoint);
    }

    private List<string> GetMethodParams(Endpoint endpoint, bool withType = true, bool withBody = true)
    {
        var methodParams = new List<string>();
        foreach (var param in endpoint.GetRouteParams())
        {
            if (withType)
            {
                methodParams.Add($"{param.GetJavaType()} {param.GetParamName()}");
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
                methodParams.Add($"{param.GetJavaType()} {param.GetParamName()}");
            }
            else
            {
                methodParams.Add(param.GetParamName());
            }
        }

        var bodyParam = endpoint.GetBodyParam();
        if (bodyParam != null && withBody)
        {
            if (withType)
            {
                methodParams.Add($"{bodyParam.GetJavaType()} {bodyParam.GetParamName()}");
            }
            else
            {
                methodParams.Add(bodyParam.GetParamName());
            }
        }

        return methodParams;
    }

    private void WriteUriBuilderMethod(JavaWriter fw, Endpoint endpoint)
    {
        fw.WriteDocStart(1, $"UriComponentsBuilder pour la méthode {endpoint.Name}");

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

        fw.WriteLine(1, $"protected {returnType} {endpoint.Name.ToFirstLower()}UriComponentsBuilder({string.Join(", ", methodParams)}) {{");
        var fullRoute = endpoint.FullRoute;
        fullRoute = "/" + fullRoute;
        foreach (IProperty p in endpoint.GetRouteParams())
        {
            fullRoute = fullRoute.Replace(@$"{{{p.GetParamName()}}}", "%s");
        }

        if (endpoint.GetRouteParams().Any())
        {
            fullRoute = $@"""{fullRoute}"".formatted({string.Join(", ", endpoint.GetRouteParams().Select(p => p.GetParamName()))});";
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
            var isRequired = p is IFieldProperty fp && fp.Required;
            if (!isRequired)
            {
                fw.WriteLine(2, @$"if ({p.GetParamName()} != null) {{");
                indentLevel++;
            }

            fw.WriteLine(indentLevel, @$"uriBuilder.queryParam(""{p.GetParamName()}"", {p.GetParamName()});");
            if (!isRequired)
            {
                fw.WriteLine(2, @$"}}");
                fw.WriteLine();
            }
        }

        fw.WriteLine(2, $"return uriBuilder;");
        fw.WriteLine(1, "}");
    }

    private void WriteEndpointCallMethod(JavaWriter fw, Endpoint endpoint)
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
            returnType = $"ResponseEntity<{endpoint.Returns.GetJavaType().Split('<').First()}>";
            returnClass = $"{endpoint.Returns.GetJavaType().Split('<').First()}.class";
            if (endpoint.Returns.GetJavaType().Split('<').First() == "ResponseEntity" && endpoint.Returns.GetJavaType().Split('<').Count() > 1)
            {
                returnType = $"ResponseEntity<{endpoint.Returns.GetJavaType().Split('<')[1].Split('>').First()}>";
                returnClass = $"{endpoint.Returns.GetJavaType().Split('<')[1].Split('>').First()}.class";
            }
        }

        var methodParams = GetMethodParams(endpoint, true, false);
        fw.WriteLine(1, $"public {returnType} {endpoint.Name.ToFirstLower()}({string.Join(", ", GetMethodParams(endpoint))}){{");
        fw.WriteLine(2, $"HttpHeaders headers = this.getHeaders();");
        fw.WriteLine(2, $"UriComponentsBuilder uri = this.{endpoint.Name.ToFirstLower()}UriComponentsBuilder({string.Join(", ", GetMethodParams(endpoint, false, false))});");
        var body = $"new HttpEntity<>({(endpoint.GetBodyParam()?.GetParamName() != null ? $"{endpoint.GetBodyParam()?.GetParamName()}, " : string.Empty)}headers)";
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

    private void WriteImports(IEnumerable<ModelFile> files, JavaWriter fw)
    {
        var imports = new List<string>();
        imports.AddRange(files.SelectMany(GetTypeImports).Distinct());
        imports.Add(_config.PersistenceMode.ToString().ToLower() + ".annotation.Generated");
        imports.Add("org.springframework.web.util.UriComponentsBuilder");
        imports.Add("org.springframework.web.client.RestTemplate");
        imports.Add("java.net.URI");
        imports.Add("org.springframework.http.HttpMethod");
        imports.Add("org.springframework.http.HttpEntity");
        imports.Add("org.springframework.http.HttpHeaders");
        imports.Add("org.springframework.http.ResponseEntity");
        fw.AddImports(imports);
    }

    private IEnumerable<string> GetTypeImports(ModelFile file)
    {
        var properties = file.Endpoints.SelectMany(endpoint => endpoint.Params).Concat(file.Endpoints.Where(endpoint => endpoint.Returns is not null).Select(endpoint => endpoint.Returns));
        return properties.SelectMany(property => property!.GetTypeImports(_config));
    }

    private void CheckEndpoint(Endpoint endpoint)
    {
        foreach (var q in endpoint.GetQueryParams().Concat(endpoint.GetRouteParams()))
        {
            if (q is AssociationProperty ap)
            {
                throw new ModelException(endpoint, $"Le endpoint {endpoint.Route} ne peut pas contenir d'association");
            }
        }

        if (endpoint.Returns != null && endpoint.Returns is AssociationProperty)
        {
            throw new ModelException(endpoint, $"Le retour du endpoint {endpoint.Route} ne peut pas être une association");
        }
    }
}