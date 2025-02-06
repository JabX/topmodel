using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.SqlServer;

public class SqlServerTypeGenerator(ILogger<SqlServerTypeGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    /// <summary>
    /// Nom pour l'insert en bulk.
    /// </summary>
    private const string InsertKeyName = "InsertKey";

    /// <summary>
    /// Type json pour les compositions.
    /// </summary>
    private const string JsonType = "json";

    public override string Name => "SqlServerTypeGen";

    protected override bool PersistentOnly => true;

    protected override IEnumerable<Class> GetExtraClasses(ModelFile file)
    {
        return file.GetExtraClasses();
    }

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract)
        {
            yield return ("type", Config.Procedural!.TypeFile!);
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writer = this.OpenSqlWriter(fileName);

        var appName = classes.First().Namespace.App;

        writer.WriteLine("-- =========================================================================================== ");
        writer.WriteLine($"--   Application Name	:	{appName} ");
        writer.WriteLine("--   Script Name		:	" + fileName?.Split('/').Last());
        writer.WriteLine("--   Description		:	Script de création des types. ");
        writer.WriteLine("-- =========================================================================================== ");

        foreach (var classe in classes.OrderBy(c => c.SqlName))
        {
            WriteTypeDeclaration(classe, writer);
        }
    }

    /// <summary>
    /// Ecrit dans le writer le script de création du type.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <param name="writer">Writer.</param>
    private static void WriteType(Class classe, IFileWriter writer)
    {
        var typeName = classe.SqlName + "_TABLE_TYPE";
        writer.WriteLine("/**");
        writer.WriteLine("  * Création du type " + classe.SqlName + "_TABLE_TYPE");
        writer.WriteLine(" **/");
        writer.WriteLine("If Exists (Select * From sys.types st Join sys.schemas ss On st.schema_id = ss.schema_id Where st.name = N'" + typeName + "')");
        writer.WriteLine("Drop Type " + typeName + '\n');
        writer.WriteLine("Create type " + typeName + " as Table (");
    }

    private void WriteTypeDeclaration(Class classe, IFileWriter writer)
    {
        var fkPropertiesList = new List<AssociationProperty>();

        var isContainsInsertKey = classe.Properties.Any(p => p.Name == InsertKeyName);
        if (isContainsInsertKey)
        {
            WriteType(classe, writer);
        }

        var properties = classe.Properties.Where(p => p is not AssociationProperty ap || ap.Type == AssociationType.ManyToOne || ap.Type == AssociationType.OneToOne).ToList();
        var t = 0;

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

        var oneToManyProperties = Classes.SelectMany(cl => cl.Properties).Where(p => p is AssociationProperty ap && ap.Type == AssociationType.OneToMany && ap.Association == classe).Select(p => (AssociationProperty)p);
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

        foreach (var property in properties)
        {
            var persistentType = property is not CompositionProperty ? Config.GetType(property, Classes) : JsonType;

            if (persistentType.ToLower().Equals("varchar") && property.Domain.Length != null)
            {
                persistentType = $"{persistentType}({property.Domain.Length})";
            }

            if ((persistentType.ToLower().Equals("numeric") || persistentType.ToLower().Equals("decimal")) && property.Domain.Length != null)
            {
                persistentType = $"{persistentType}({property.Domain.Length}{(property.Domain.Scale != null ? $", {property.Domain.Scale}" : string.Empty)})";
            }

            if (isContainsInsertKey && !property.PrimaryKey && property.Name != InsertKeyName)
            {
                if (t > 0)
                {
                    writer.Write(",");
                    writer.WriteLine();
                }

                writer.Write("\t" + property.SqlName + " " + persistentType);
                t++;
            }
        }

        if (isContainsInsertKey)
        {
            if (t > 0)
            {
                writer.Write(",");
                writer.WriteLine();
            }

            writer.WriteLine('\t' + classe.Trigram + "_INSERT_KEY int");
            writer.WriteLine();
            writer.WriteLine($"){Config.BatchSeparator}");
            writer.WriteLine();
        }
    }
}
