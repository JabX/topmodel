using Microsoft.Extensions.Logging;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa.EndpointGeneration;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class FeignClientApiGenerator(ILogger<FeignClientApiGenerator> logger, IFileWriterProvider writerProvider)
    : SpringServerApiGenerator(logger, writerProvider)
{
    public override string Name => "FeignClientApiGen";

    protected override bool FilterTag(string tag)
    {
        return Config.ResolveVariables(Config.ApiGeneration!, tag) == ApiGeneration.Client && Config.ResolveVariables(Config.ClientApiGeneration!, tag) == ClientApiMode.FeignClient;
    }

    protected override IEnumerable<JavaAnnotation> GetClassAnnotations(ModelFile file)
    {
        var fileName = file.Options.Endpoints.FileName;
        foreach (var a in base.GetClassAnnotations(file).Where(a => a.Name != "RequestMapping"))
        {
            yield return a;
        }

        var feignClientAnnotation = new JavaAnnotation("FeignClient", imports: "org.springframework.cloud.openfeign.FeignClient")
                         .AddAttribute("name", $@"""{file.Namespace.RootModule}""")
                         .AddAttribute("contextId", $@"""{GetClassName(fileName)}""");

        if (!string.IsNullOrEmpty(file.Options.Endpoints.Prefix))
        {
            feignClientAnnotation.AddAttribute("path", $@"""{file.Options.Endpoints.Prefix}""");
        }

        yield return feignClientAnnotation;
    }

    protected override string GetClassName(string fileName)
    {
        return $"{fileName.ToPascalCase()}Api";
    }
}
