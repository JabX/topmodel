using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class SpringServerApiGenerator : EndpointsGeneratorBase<JpaConfig>
{
    private readonly ILogger<SpringServerApiGenerator> _logger;

    public SpringServerApiGenerator(ILogger<SpringServerApiGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "SpringApiServerGen";

    protected virtual void AddImports(IEnumerable<Endpoint> endpoints, JavaWriter fw, string tag)
    {
        fw.AddImports(GetTypeImports(endpoints, tag));
        fw.AddImports(endpoints.SelectMany(e => Config.GetDecoratorImports(e, tag)));
    }

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Server;
    }

    protected virtual IEnumerable<JavaAnnotation> GetClassAnnotations(ModelFile file)
    {
        if (Config.GeneratedHint)
        {
            yield return Config.GeneratedAnnotation;
        }

        if (file.Options.Endpoints.Prefix != null)
        {
            yield return new JavaAnnotation("RequestMapping", $@"""{file.Options.Endpoints.Prefix}""", "org.springframework.web.bind.annotation.RequestMapping");
        }
    }

    protected virtual string GetClassName(string fileName)
    {
        return $"{fileName.ToPascalCase()}Controller";
    }

    protected override string GetFilePath(ModelFile file, string tag)
    {
        return Path.Combine(Config.GetApiPath(file, tag), $"{GetClassName(file.Options.Endpoints.FileName)}.java");
    }

    protected virtual IEnumerable<string> GetTypeImports(IEnumerable<Endpoint> endpoints, string tag)
    {
        var properties = endpoints.SelectMany(endpoint => endpoint.Params)
            .Concat(endpoints.Where(endpoint => endpoint.Returns is not null)
            .Select(endpoint => endpoint.Returns));
        return properties.SelectMany(property => property!.GetTypeImports(Config, tag))
                .Concat(endpoints.Where(endpoint => endpoint.Returns is not null)
                .Select(e => e.Returns).OfType<CompositionProperty>()
                .SelectMany(c => c.GetKindImports(Config, tag)));
    }

    protected override void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints)
    {
        var className = GetClassName(fileName);
        var packageName = Config.GetPackageName(endpoints.First(), tag);
        using var fw = new JavaWriter(filePath, _logger, packageName, null);

        AddImports(endpoints, fw, tag);
        fw.WriteLine();
        var javaxOrJakarta = Config.PersistenceMode.ToString().ToLower();
        fw.WriteAnnotations(0, GetClassAnnotations(endpoints.First().ModelFile));
        fw.WriteLine($"public interface {className} {{");

        foreach (var endpoint in endpoints)
        {
            WriteEndpoint(fw, endpoint, tag);
        }

        fw.WriteLine("}");
    }

    protected virtual void WriteEndpoint(JavaWriter fw, Endpoint endpoint, string tag)
    {
        fw.WriteLine();
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
        var returnType = "void";

        if (endpoint.Returns != null)
        {
            returnType = Config.GetType(endpoint.Returns);
        }

        {
            var mappingAnnotation = new JavaAnnotation($@"@{endpoint.Method.ToPascalCase(true)}Mapping", imports: $"org.springframework.web.bind.annotation.{endpoint.Method.ToPascalCase(true)}Mapping")
                .AddAttribute("path", $@"""{endpoint.Route}""");
            if (endpoint.Returns != null && endpoint.Returns.Domain?.MediaType != null)
            {
                mappingAnnotation.AddAttribute("produces", @$"""{endpoint.Returns.Domain.MediaType}""");
            }

            var consumes = string.Empty;
            if (endpoint.Params.Any(p => p.Domain?.MediaType != null))
            {
                mappingAnnotation.AddAttribute("consumes", @$"{{ {string.Join(", ", endpoint.Params.Where(p => p.Domain?.MediaType != null).Select(p => $@"""{p.Domain.MediaType}"""))} }}");
            }

            foreach (var annotation in Config.GetDecoratorAnnotations(endpoint, tag))
            {
                fw.WriteLine(1, $"{(annotation.StartsWith('@') ? string.Empty : "@")}{annotation}");
            }

            fw.WriteLine(1, mappingAnnotation);
        }

        var methodParams = new List<string>();
        var method = new JavaMethod(returnType, endpoint.NameCamel);
        foreach (var param in endpoint.GetRouteParams())
        {
            var pathParamAnnotation = @$"@PathVariable(""{param.GetParamName()}"")";
            fw.AddImport("org.springframework.web.bind.annotation.PathVariable");
            fw.AddImports(Config.GetDomainImports(param, tag));
            var decoratorAnnotations = string.Join(' ', Config.GetDomainAnnotations(param, tag).Select(a => a.StartsWith('@') ? a : "@" + a));
            methodParams.Add($"{(pathParamAnnotation.Length > 0 ? $"{pathParamAnnotation} " : string.Empty)}{(decoratorAnnotations.Length > 0 ? $"{decoratorAnnotations} " : string.Empty)}{Config.GetType(param)} {param.GetParamName()}");
        }

        foreach (var param in endpoint.GetQueryParams())
        {
            var ann = string.Empty;
            ann += @$"@RequestParam(value = ""{param.GetParamName()}"", required = {param.Required.ToString().ToFirstLower()}) ";
            fw.AddImport("org.springframework.web.bind.annotation.RequestParam");
            fw.AddImports(Config.GetDomainImports(param, tag));
            var decoratorAnnotations = string.Join(' ', Config.GetDomainAnnotations(param, tag).Select(a => a.StartsWith("@") ? a : "@" + a));
            methodParams.Add($"{ann}{(decoratorAnnotations.Length > 0 ? $" {decoratorAnnotations}" : string.Empty)}{Config.GetType(param)} {param.GetParamName()}");
        }

        if (endpoint.IsMultipart)
        {
            foreach (var param in endpoint.Params.Where(param => param is CompositionProperty || (param.Domain?.BodyParam ?? false) || (param.Domain?.IsMultipart ?? false)))
            {
                var ann = string.Empty;
                if (!(param.Domain?.IsMultipart ?? false))
                {
                    ann += @$"@ModelAttribute ";
                    fw.AddImport("org.springframework.web.bind.annotation.ModelAttribute");
                }
                else
                {
                    ann += @$"@RequestPart(value = ""{param.GetParamName()}"", required = {param.Required.ToString().ToFirstLower()}) ";
                    fw.AddImport("org.springframework.web.bind.annotation.RequestPart");
                }

                methodParams.Add($"{ann}{Config.GetType(param)} {param.GetParamName()}");
            }
        }
        else
        {
            var bodyParam = endpoint.GetJsonBodyParam();
            if (bodyParam != null)
            {
                var ann = string.Empty;
                ann += @$"@RequestBody @Valid ";
                fw.AddImport("org.springframework.web.bind.annotation.RequestBody");
                fw.AddImport(Config.PersistenceMode.ToString().ToLower() + ".validation.Valid");
                methodParams.Add($"{ann}{Config.GetType(bodyParam)} {bodyParam.GetParamName()}");
            }
        }

        fw.WriteLine(1, $"{returnType} {endpoint.NameCamel}({string.Join(", ", methodParams)});");
    }
}
