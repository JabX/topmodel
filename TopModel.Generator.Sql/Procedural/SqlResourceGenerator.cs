using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural;

public class SqlResourceGenerator(ILogger<SqlResourceGenerator> logger, TranslationStore translationStore, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    public override string Name => "SqlResourceGen";

    protected override bool PersistentOnly => true;

    protected override IEnumerable<Class> GetExtraClasses(ModelFile file)
    {
        return file.GetExtraClasses();
    }

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract)
        {
            yield return ("resource", Config.Procedural!.ResourceFile!);
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writer = this.OpenSqlWriter(fileName);

        var appName = classes.First().Namespace.App;

        writer.WriteLine("-- =========================================================================================== ");
        writer.WriteLine($"--   Application Name	:	{appName} ");
        writer.WriteLine("--   Script Name		:	" + fileName?.Split('/').Last());
        writer.WriteLine("--   Description		:	Script de création des resources (libellés traduits). ");
        writer.WriteLine("-- =========================================================================================== ");

        var propertiesMap = classes
            .OrderBy(c => c.SqlName)
            .Where(c => c != null && c.Properties != null)
            .SelectMany(c => c.Properties)
            .Where(p => p.ResourceProperty.Parent.Namespace.Module != null && p.Label != null && p.ResourceProperty != null && p.Class != null)
            .DistinctBy(property => property.ResourceKey).GroupBy(property => property.Class).ToDictionary(g => g.Key, g => g.Select(t => t));

        foreach (var modelClass in propertiesMap.Keys)
        {
            if (propertiesMap.TryGetValue(modelClass, out var properties))
            {
                var hasLocale = translationStore.Translations.Keys.Count > 1 || translationStore.Translations.Keys.Any(a => a != string.Empty);
                if (Config.TranslateProperties == true && properties.Any(p => p.Label != null) && modelClass.ModelFile != null)
                {
                    writer.WriteLine();
                    writer.WriteLine("/**\t\tInitialisation des traductions des propriétés de la table " + modelClass.SqlName + "\t\t**/");

                    foreach (var lang in translationStore.Translations.Keys)
                    {
                        foreach (var property in properties.Where(p => p.Label != null).DistinctBy(property => property.ResourceKey))
                        {
                            writer.WriteLine($@"INSERT INTO {Config.ResourcesTableName}(RESOURCE_KEY{(hasLocale ? ", LOCALE" : string.Empty)}, LABEL) VALUES({SingleQuote(property.ResourceKey)}{(string.IsNullOrEmpty(lang) ? string.Empty : @$", {SingleQuote(lang)}")}, {SingleQuote(translationStore.GetTranslation(property, lang))});");
                        }
                    }
                }

                if (modelClass.DefaultProperty != null && modelClass.Values.Count > 0 && Config.TranslateReferences == true)
                {
                    writer.WriteLine();
                    writer.WriteLine("/**\t\tInitialisation des traductions des valeurs de la table " + modelClass.SqlName + "\t\t**/");
                    foreach (var lang in translationStore.Translations.Keys)
                    {
                        foreach (var val in modelClass.Values)
                        {
                            writer.WriteLine(@$"INSERT INTO {Config.ResourcesTableName}(RESOURCE_KEY{(hasLocale ? ", LOCALE" : string.Empty)}, LABEL) VALUES({SingleQuote(val.ResourceKey)}{(string.IsNullOrEmpty(lang) ? string.Empty : @$", {SingleQuote(lang)}")}, {SingleQuote(translationStore.GetTranslation(val, lang))});");
                        }
                    }
                }
            }
        }
    }

    private static string SingleQuote(string name)
    {
        return $@"'{name.Replace("'", "''")}'";
    }
}
