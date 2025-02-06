using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql;

/// <summary>
/// Classe utilitaire pour écritre du SQL.
/// </summary>
public static class ScriptUtils
{
    public const string InsertKeyName = "InsertKey";

    public static IList<IProperty> GetAllProperties(this Class classe, IEnumerable<Class> availableClasses)
    {
        var properties = classe.Properties.Where(p => p is not AssociationProperty ap || ap.Type == AssociationType.ManyToOne || ap.Type == AssociationType.OneToOne).ToList();

        if (classe.Extends != null)
        {
            properties.Add(new AssociationProperty
            {
                Association = classe.Extends,
                Class = classe,
                Comment = "Association vers la clé primaire de la classe parente",
                Required = true,
                PrimaryKey = !classe.PrimaryKey.Any()
            });
        }

        var oneToManyProperties = availableClasses.SelectMany(cl => cl.Properties).Where(p => p is AssociationProperty ap && ap.Type == AssociationType.OneToMany && ap.Association == classe).Select(p => (AssociationProperty)p);
        foreach (var ap in oneToManyProperties)
        {
            var asp = new AssociationProperty()
            {
                Association = ap.Class,
                Class = ap.Association,
                Comment = ap.Comment,
                Type = AssociationType.ManyToOne,
                Required = ap.Required,
                Role = ap.Role,
                DefaultValue = ap.DefaultValue,
                Label = ap.Label
            };
            properties.Add(asp);
        }

        return properties;
    }

    public static IEnumerable<Class> GetExtraClasses(this ModelFile file)
    {
        var manyToManyProperties = file.Classes
            .Where(c => c.IsPersistent && !c.Abstract)
            .SelectMany(cl => cl.Properties)
            .OfType<AssociationProperty>()
            .Where(ap => ap.Type == AssociationType.ManyToMany);

        foreach (var ap in manyToManyProperties)
        {
            var traClass = new Class
            {
                Comment = ap.Comment,
                Label = ap.Label,
                SqlName = $"{ap.Class.SqlName}_{ap.Association.SqlName}{(ap.Role != null ? $"_{ap.Role.ToConstantCase()}" : string.Empty)}",
                ModelFile = file
            };

            traClass.Properties.Add(new AssociationProperty
            {
                Association = ap.Class,
                Class = traClass,
                Comment = ap.Comment,
                Type = AssociationType.ManyToOne,
                PrimaryKey = true,
                Required = true,
                Role = ap.Role,
                DefaultValue = ap.DefaultValue,
                Label = ap.Label,
                Trigram = ap.Class.PrimaryKey.Single().Trigram
            });

            traClass.Properties.Add(new AssociationProperty
            {
                Association = ap.Association,
                Class = traClass,
                Comment = ap.Comment,
                Type = AssociationType.ManyToOne,
                PrimaryKey = true,
                Required = true,
                Role = ap.Role,
                DefaultValue = ap.DefaultValue,
                Label = ap.Label,
                Trigram = ap.Trigram ?? ap.Property.Trigram ?? ap.Association.Trigram
            });

            yield return traClass;
        }
    }

    /// <summary>
    /// Retourne le nom du type de table SQL correspondant à la classe.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <returns>Nom du type de table.</returns>
    public static string GetTableTypeName(this Class classe)
    {
        return classe == null
            ? throw new ArgumentNullException(nameof(classe))
            : classe.SqlName + "_TABLE_TYPE";
    }

    public static IFileWriter OpenSqlWriter(this GeneratorBase<SqlConfig> generator, string fileName)
    {
        var fw = generator.OpenFileWriter(fileName);
        fw.StartCommentToken = "----";
        return fw;
    }
}