using System.Text;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Core;

public abstract class GeneratorBase<T> : IModelWatcher
    where T : GeneratorConfigBase
{
    private readonly ILogger _logger;
    private readonly IFileWriterProvider? _writerProvider;

    [Obsolete("Utiliser la surcharge avec le IFileWriterProvider")]
    protected GeneratorBase(ILogger logger)
    {
        _logger = logger;
    }

    protected GeneratorBase(ILogger logger, IFileWriterProvider writerProvider)
    {
        _logger = logger;
        _writerProvider = writerProvider;
    }

    public abstract string Name { get; }

#nullable disable
    public T Config { get; internal set; }
#nullable enable

    public int Number { get; internal set; }

    public virtual IEnumerable<string> GeneratedFiles => [];

    public bool Disabled => Config.Disable?.Contains(Name) ?? false;

    protected Dictionary<string, ModelFile> Files { get; } = [];

    protected IEnumerable<Class> Classes => Files
        .SelectMany(f => f.Value.Classes.Where(c => Config.Tags.Intersect(c.Tags).Any()).Concat(GetExtraClasses(f.Value)))
        .Distinct();

    protected virtual bool PersistentOnly => false;

    protected virtual bool NoLanguage => false;

    /// <inheritdoc cref="IModelWatcher.OnErrors" />
    public void OnErrors(IDictionary<ModelFile, IEnumerable<ModelError>> errors)
    {
    }

    /// <inheritdoc cref="IModelWatcher.OnFilesChanged" />
    public void OnFilesChanged(IEnumerable<ModelFile> files, LoggingScope? storeConfig = null)
    {
        using var scope = _logger.BeginScope(((IModelWatcher)this).FullName);
        using var scope2 = _logger.BeginScope(storeConfig!);

        var handledFiles = files.Where(file => Config.Tags.Intersect(file.AllTags.Except(Config.ExcludedTags)).Any());

        if (!NoLanguage)
        {
            var missingDomains = handledFiles.SelectMany(f => f.Properties).Where(fp => !PersistentOnly || (fp.Class?.IsPersistent ?? false)).Select(fp => fp.Domain)
                .Concat(PersistentOnly ? [] : handledFiles.SelectMany(f => f.Properties).OfType<CompositionProperty>().Select(fp => fp.Domain!))
                .Where(domain => domain != null && Config.GetImplementation(domain) == null)
                .Distinct();

            if (missingDomains.Any())
            {
                throw new ModelException($"Pour utiliser le générateur '{Name}', les domaines suivants doivent définir une implémentation pour '{Config.Language}' : {string.Join(", ", missingDomains.Select(d => d.Name).OrderBy(x => x))}.");
            }
        }

        foreach (var file in handledFiles)
        {
            Files[file.Name] = file;
        }

        HandleFiles(handledFiles);
    }

    public IFileWriter OpenFileWriter(string fileName, bool encoderShouldEmitUTF8Identifier = true)
    {
        if (_writerProvider == null)
        {
            throw new NotImplementedException();
        }

        return _writerProvider.OpenFileWriter(Path.Combine(Config.OutputDirectory, fileName).Replace("\\", "/"), _logger, encoderShouldEmitUTF8Identifier);
    }

    public IFileWriter OpenFileWriter(string fileName, Encoding encoding)
    {
        if (_writerProvider == null)
        {
            throw new NotImplementedException();
        }

        return _writerProvider.OpenFileWriter(Path.Combine(Config.OutputDirectory, fileName).Replace("\\", "/"), _logger, encoding);
    }

    protected IEnumerable<ClassValue> GetAllValues(Class classe)
    {
        foreach (var value in classe.Values)
        {
            yield return value;
        }

        foreach (var child in Classes.Where(c => c.Extends == classe))
        {
            foreach (var value in GetAllValues(child))
            {
                yield return value;
            }
        }
    }

    protected string GetBestClassTag(Class classe, string tag)
    {
        return classe.Tags.Contains(tag) ? tag : classe.Tags.Intersect(Config.Tags).FirstOrDefault() ?? tag;
    }

    protected virtual IEnumerable<Class> GetExtraClasses(ModelFile file)
    {
        return [];
    }

    protected abstract void HandleFiles(IEnumerable<ModelFile> files);
}