using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Core;

public abstract class EndpointsGeneratorBase<T> : GeneratorBase<T>
    where T : GeneratorConfigBase
{
    [Obsolete("Utiliser la surcharge avec le GeneratedFileWriterProvider")]
    public EndpointsGeneratorBase(ILogger<EndpointsGeneratorBase<T>> logger)
        : base(logger)
    {
    }

    public EndpointsGeneratorBase(ILogger<EndpointsGeneratorBase<T>> logger, GeneratedFileWriterProvider writerProvider)
        : base(logger, writerProvider)
    {
    }

    public override List<string> GeneratedFiles => Files.Values
        .SelectMany(file => Config.Tags.Intersect(file.AllTags.Where(FilterTag)).Select(tag => (file, path: GetFilePath(file, tag))))
        .Where(i => i.file.Endpoints.Any())
        .Select(i => i.path)
        .Distinct()
        .ToList();

    protected virtual bool FilterTag(string tag)
    {
        return true;
    }

    protected abstract string GetFilePath(ModelFile file, string tag);

    protected abstract void HandleFile(string filePath, string fileName, string tag, IList<Endpoint> endpoints);

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        Parallel.ForEach(
            Files.Values
                .SelectMany(file => Config.Tags.Intersect(file.AllTags.Where(FilterTag))
                    .Select(tag => (tag, file, filePath: GetFilePath(file, tag))))
                .GroupBy(file => file.filePath),
            file =>
            {
                var endpoints = file
                    .SelectMany(f => f.file.Endpoints)
                    .Where(e => e.Tags.Intersect(file.Select(f => f.tag)).Any())
                    .Distinct()
                    .OrderBy(e => e.Name, StringComparer.Ordinal)
                    .ToList();

                if (endpoints.Any())
                {
                    HandleFile(file.Key, file.First().file.Options.Endpoints.FileName, file.First().tag, endpoints);
                }
            });
    }
}