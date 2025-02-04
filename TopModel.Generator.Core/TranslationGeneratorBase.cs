using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Core;

public abstract class TranslationGeneratorBase<T> : GeneratorBase<T>
    where T : GeneratorConfigBase
{
    private readonly TranslationStore _translationStore;

    [Obsolete("Utiliser la surcharge avec le GeneratedFileWriterProvider")]
    public TranslationGeneratorBase(ILogger<TranslationGeneratorBase<T>> logger, TranslationStore translationStore)
        : base(logger)
    {
        _translationStore = translationStore;
    }

    public TranslationGeneratorBase(ILogger<TranslationGeneratorBase<T>> logger, TranslationStore translationStore, GeneratedFileWriterProvider writerProvider)
        : base(logger, writerProvider)
    {
        _translationStore = translationStore;
    }

    public override IEnumerable<string> GeneratedFiles => Config.Tags
        .SelectMany(tag =>
        {
            var properties = Classes
                .Where(c => c.Tags.Contains(tag))
                .SelectMany(c => c.Properties);

            return properties
                .SelectMany(p => GetResourceFileNames(p, tag))
                .Concat(properties.SelectMany(p => GetCommentResourceFileNames(p, tag)))
                .Concat(GetMainResourceFileNames(tag))
                .Select(p => p.FilePath);
        })
        .Distinct();

    protected virtual string? GetCommentResourceFilePath(IProperty property, string tag, string lang)
    {
        return null;
    }

    protected virtual string? GetMainResourceFilePath(string tag, string lang)
    {
        return null;
    }

    protected abstract string? GetResourceFilePath(IProperty property, string tag, string lang);

    protected virtual void HandleCommentResourceFile(string filePath, string lang, IEnumerable<IProperty> properties)
    {
    }

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        var modules = new List<(string MainFilePath, string ModuleFilePath, string ModuleName)>();

        Parallel.ForEach(
            Classes
                .SelectMany(classe => Config.Tags.Intersect(classe.Tags)
                    .SelectMany(tag => classe.Properties.OfType<IProperty>()
                        .SelectMany(p => GetResourceFileNames(p, tag)
                            .Select(f => (key: (MainFilePath: GetMainResourceFilePath(tag, f.Lang), ModuleFilePath: f.FilePath, f.Lang), p)))))
                    .GroupBy(f => f.key),
            resources =>
            {
                var properties = resources.Select(r => r.p.ResourceProperty).Distinct();
                HandleResourceFile(resources.Key.ModuleFilePath, resources.Key.Lang, properties);

                if (resources.Key.MainFilePath != null)
                {
                    modules.Add((resources.Key.MainFilePath, resources.Key.ModuleFilePath, properties.First().Parent.Namespace.RootModule));
                }
            });

        Parallel.ForEach(
            Classes
                .SelectMany(classe => Config.Tags.Intersect(classe.Tags)
                    .SelectMany(tag => classe.Properties.OfType<IProperty>()
                        .SelectMany(p => GetCommentResourceFileNames(p, tag)
                            .Select(f => (key: (MainFilePath: GetMainResourceFilePath(tag, f.Lang), ModuleFilePath: f.FilePath, f.Lang), p)))))
                    .GroupBy(f => f.key),
            resources =>
            {
                var properties = resources.Select(r => r.p.CommentResourceProperty).Distinct();
                HandleCommentResourceFile(resources.Key.ModuleFilePath, resources.Key.Lang, properties);

                if (resources.Key.MainFilePath != null)
                {
                    modules.Add((resources.Key.MainFilePath, resources.Key.ModuleFilePath, $"{properties.First().Parent.Namespace.RootModule}Comments"));
                }
            });

        Parallel.ForEach(
            modules.GroupBy(m => m.MainFilePath),
            g => HandleMainResourceFile(
                g.Key,
                g.Select(l => (l.ModuleFilePath, l.ModuleName)).OrderBy(m => m.ModuleFilePath)));
    }

    protected virtual void HandleMainResourceFile(string mainFilePath, IEnumerable<(string ModuleFilePath, string ModuleName)> modules)
    {
    }

    protected abstract void HandleResourceFile(string filePath, string lang, IEnumerable<IProperty> properties);

    private IEnumerable<(string Lang, string FilePath)> GetCommentResourceFileNames(IProperty property, string tag)
    {
        if (Config.TranslateProperties != true)
        {
            return [];
        }

        return _translationStore.Translations
            .Select(lang => (lang: lang.Key, file: GetCommentResourceFilePath(property.CommentResourceProperty, tag, lang.Key)!))
            .Where(g => g.file != null);
    }

    private IEnumerable<(string Lang, string FilePath)> GetMainResourceFileNames(string tag)
    {
        return _translationStore.Translations
            .Select(lang => (lang: lang.Key, file: GetMainResourceFilePath(tag, lang.Key)!))
            .Where(g => g.file != null);
    }

    private IEnumerable<(string Lang, string FilePath)> GetResourceFileNames(IProperty property, string tag)
    {
        if (Config.TranslateProperties != true && (Config.TranslateReferences != true || !(property.Class?.Values.Any() ?? false)))
        {
            return [];
        }

        return _translationStore.Translations
            .Select(lang => (lang: lang.Key, file: GetResourceFilePath(property.ResourceProperty, tag, lang.Key)!))
            .Where(g => g.file != null);
    }
}