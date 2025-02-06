using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class JavascriptResourceGenerator(ILogger<JavascriptResourceGenerator> logger, TranslationStore translationStore, ModelConfig modelConfig, IFileWriterProvider writerProvider)
    : TranslationGeneratorBase<JavascriptConfig>(logger, translationStore, writerProvider)
{
    private readonly TranslationStore _translationStore = translationStore;

    public override string Name => "JSResourceGen";

    protected override string? GetCommentResourceFilePath(IProperty property, string tag, string lang)
    {
        if (!Config.GenerateComments)
        {
            return null;
        }

        return Config.GetCommentResourcesFilePath(property.Parent.Namespace, tag, modelConfig.I18n.DefaultLang);
    }

    protected override string? GetMainResourceFilePath(string tag, string lang)
    {
        if (!Config.GenerateMainResourceFiles || Config.ResourceMode != ResourceMode.JS)
        {
            return null;
        }

        return Config.GetMainResourceFilePath(tag, lang);
    }

    protected override string GetResourceFilePath(IProperty property, string tag, string lang)
    {
        return Config.GetResourcesFilePath(property.Parent.Namespace, tag, lang);
    }

    protected override void HandleCommentResourceFile(string filePath, string lang, IEnumerable<IProperty> properties)
    {
        using var fw = OpenFileWriter(filePath, false);
        fw.EnableHeader = Config.ResourceMode == ResourceMode.JS;

        var module = properties.First().Parent.Namespace.RootModule;

        if (Config.ResourceMode != ResourceMode.JS)
        {
            fw.WriteLine("{");
        }
        else
        {
            fw.WriteLine($"export const {module.ToCamelCase()}Comments = {{");
        }

        WriteSubModule(fw, modelConfig.I18n.DefaultLang, properties.Where(p => Config.ExtendedCompositions || p is not CompositionProperty and not AliasProperty { Property: CompositionProperty }), true, 1);

        if (Config.ResourceMode != ResourceMode.JS)
        {
            fw.WriteLine("}");
        }
        else
        {
            fw.WriteLine("};");
        }
    }

    protected override void HandleMainResourceFile(string mainFilePath, IEnumerable<(string ModuleFilePath, string ModuleName)> modules)
    {
        using var fw = OpenFileWriter(mainFilePath, false);

        foreach (var (moduleFilePath, moduleName) in modules)
        {
            fw.WriteLine($"import {{{moduleName.ToCamelCase()}}} from \"./{Path.GetRelativePath(Path.GetDirectoryName(mainFilePath)!, moduleFilePath).Replace("\\", "/").Replace(".ts", string.Empty)}\";");
        }

        fw.WriteLine();
        fw.WriteLine($"export const all = {{{string.Join(", ", modules.Where(m => !m.ModuleFilePath.EndsWith(".comments.ts")).Select(m => m.ModuleName.ToCamelCase()))}}};");

        var comments = modules.Where(m => m.ModuleFilePath.EndsWith(".comments.ts")).Select(m => m.ModuleName.ToCamelCase());
        if (comments.Any())
        {
            fw.WriteLine($@"export const allComments = {{
    {string.Join($",{Environment.NewLine}    ", comments.Select(c => $"{c[0..^8]}: {c}"))}
}};");
        }
    }

    protected override void HandleResourceFile(string filePath, string lang, IEnumerable<IProperty> properties)
    {
        using var fw = OpenFileWriter(filePath, false);
        fw.EnableHeader = Config.ResourceMode == ResourceMode.JS;

        var module = properties.First().Parent.Namespace.RootModule;

        if (Config.ResourceMode != ResourceMode.JS)
        {
            fw.WriteLine("{");
        }
        else
        {
            fw.WriteLine($"export const {module.ToCamelCase()} = {{");
        }

        WriteSubModule(fw, lang, properties.Where(p => Config.ExtendedCompositions || p is not CompositionProperty and not AliasProperty { Property: CompositionProperty }), false, 1);

        if (Config.ResourceMode != ResourceMode.JS)
        {
            fw.WriteLine("}");
        }
        else
        {
            fw.WriteLine("};");
        }
    }

    private string Quote(string name)
    {
        return Config.ResourceMode == ResourceMode.JS ? name : $@"""{name}""";
    }

    private void WriteClasseNode(IFileWriter fw, IGrouping<IPropertyContainer, IProperty> container, bool isComment, bool isLast, string lang, int indentLevel, bool onlyProperties = false)
    {
        if (!onlyProperties)
        {
            fw.WriteLine(indentLevel, $"{Quote(container.Key.NameCamel)}: {{");
        }

        var i = 1;
        if (Config.TranslateProperties == true)
        {
            foreach (var property in container.OrderBy(p => p.NameCamel, StringComparer.Ordinal))
            {
                var translation = isComment
                    ? property.CommentResourceProperty.Comment.Replace(Environment.NewLine, " ").Replace("\"", "'")
                    : _translationStore.GetTranslation(property, lang);

                if (translation == string.Empty)
                {
                    translation = property.Name;
                }

                fw.Write(indentLevel + 1, $"{Quote(property.NameCamel)}: ");
                fw.Write($@"""{translation}""");
                fw.WriteLine(container.Count() == i++ && !onlyProperties && !(Config.TranslateReferences == true && container.Key is Class { DefaultProperty: not null, Enum: true } && container.Key is Class { Values.Count: > 0 }) ? string.Empty : ",");
            }
        }

        if (Config.TranslateReferences == true && container.Key is Class { DefaultProperty: not null, Enum: true } classe && classe?.Values.Count > 0)
        {
            i = 1;
            fw.WriteLine(indentLevel + 1, @$"{Quote("values")}: {{");
            foreach (var refValue in classe.Values)
            {
                fw.Write(indentLevel + 2, $@"{Quote(refValue.Name)}: ");
                fw.Write($@"""{_translationStore.GetTranslation(refValue, lang)}""");
                fw.WriteLine(classe.Values.Count == i++ ? string.Empty : ",");
            }

            fw.WriteLine(indentLevel + 1, "}");
        }

        if (!onlyProperties)
        {
            fw.Write(indentLevel, "}");
            fw.WriteLine(!isLast ? "," : string.Empty);
        }
    }

    private void WriteSubModule(IFileWriter fw, string lang, IEnumerable<IProperty> properties, bool isComment, int level)
    {
        var classes = properties.GroupBy(prop => prop.Parent);
        var modules = classes
            .GroupBy(c => c.Key.Namespace.Module.Split('.').Skip(level).ElementAtOrDefault(0));
        var u = 1;

        var mainModuleClasses = modules.Where(c => c.Key == null).SelectMany(c => c.Select(p => p.Key.NameCamel)).ToHashSet();
        var extraSubModuleProperties = new Dictionary<string, IGrouping<IPropertyContainer, IProperty>>();

        if (mainModuleClasses.Count > 0)
        {
            var keys = modules.Select(m => m.Key!).Where(m => m != null).ToList();
            foreach (var key in keys)
            {
                if (mainModuleClasses.Contains(key))
                {
                    extraSubModuleProperties.Add(key, modules.Where(m => m.Key == null).SelectMany(p => p.Where(c => c.Key.NameCamel == key)).Single());
                }
            }
        }

        foreach (var submodule in modules.OrderBy(m => m.Key, StringComparer.Ordinal))
        {
            var isLast = u++ == modules.Count();
            if (submodule.Key == null)
            {
                var i = 1;
                foreach (var container in submodule.Where(c => !extraSubModuleProperties.ContainsKey(c.Key.NameCamel)).OrderBy(c => c.Key.NameCamel))
                {
                    WriteClasseNode(fw, container, isComment, classes.Count() == i++ && isLast, lang, level);
                }
            }
            else
            {
                fw.WriteLine(level, $@"{Quote(submodule.Key.Split('.').First().ToCamelCase())}: {{");

                if (extraSubModuleProperties.TryGetValue(submodule.Key, out var container))
                {
                    WriteClasseNode(fw, container, isComment, false, lang, level, onlyProperties: true);
                }

                WriteSubModule(fw, lang, submodule.SelectMany(m => m), isComment, level + 1);
                if (isLast)
                {
                    fw.WriteLine(level, "}");
                }
                else
                {
                    fw.WriteLine(level, "},");
                }
            }
        }
    }
}