using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural;

public abstract class AbstractCrebasGenerator(ILogger<AbstractCrebasGenerator> logger, TranslationStore translationStore, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    protected override bool PersistentOnly => true;

    /// <summary>
    /// Type json pour les compositions.
    /// </summary>
    protected virtual string JsonType => "json";

    /// <summary>
    /// Indique si le moteur de BDD visé supporte "primary key clustered ()".
    /// </summary>
    protected abstract bool SupportsClusteredKey
    {
        get;
    }

    /// <summary>
    /// Indique la limite de longueur d'un identifiant.
    /// </summary>
    private static int IdentifierLengthLimit => 128;

    /// <summary>
    /// Lève une ArgumentException si l'identifiant est trop long.
    /// </summary>
    /// <param name="identifier">Identifiant à vérifier.</param>
    /// <returns>Identifiant passé en paramètre.</returns>
    protected static string CheckIdentifierLength(string identifier)
    {
        return identifier.Length > IdentifierLengthLimit
            ? throw new ArgumentException($"Le nom {identifier} est trop long ({identifier.Length} caractères). Limite: {IdentifierLengthLimit} caractères.")
            : identifier;
    }

    protected override IEnumerable<Class> GetExtraClasses(ModelFile file)
    {
        return file.GetExtraClasses();
    }

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract)
        {
            yield return ("crebas", Config.Procedural!.CrebasFile!);
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writer = this.OpenSqlWriter(fileName);

        var appName = classes.First().Namespace.App;

        writer.WriteLine("-- =========================================================================================== ");
        writer.WriteLine($"--   Application Name	:	{appName} ");
        writer.WriteLine("--   Script Name		:	" + fileName.Split('/').Last());
        writer.WriteLine("--   Description		:	Script de création des tables.");
        writer.WriteLine("-- =========================================================================================== ");

        foreach (var classe in classes.OrderBy(c => c.SqlName))
        {
            WriteTableDeclaration(classe, writer);
        }

        if (Config.TranslateProperties == true || Config.TranslateReferences == true)
        {
            WriteResourceTableDeclaration(writer);
        }
    }

    protected virtual void WriteBooleanCheckConstraints(IFileWriter writer, IList<IProperty> properties)
    {
    }

    /// <summary>
    /// Gère l'auto-incrémentation des clés primaires.
    /// </summary>
    /// <param name="writer">Flux d'écriture création bases.</param>
    protected abstract void WriteIdentityColumn(IFileWriter writer);

    protected virtual void WriteSequenceDeclaration(Class classe, IFileWriter writer, string tableName)
        => throw new NotImplementedException($"Sequence declaration is not implemented with {Config.TargetDBMS}");

    private string GetTableTablespaceDeclaration() => GetTablespaceDeclaration(Config.TableTablespace);

    private string GetTablespaceDeclaration(string? tablespace)
    {
        bool ShouldGenerateTablespace()
        {
            if (!Config.AllowTablespace)
            {
                return false;
            }

            if (string.IsNullOrEmpty(tablespace))
            {
                return false;
            }

            return true;
        }

        if (ShouldGenerateTablespace())
        {
            return $"\r\nTABLESPACE  {tablespace} ";
        }

        return string.Empty;
    }

    /// <summary>
    /// Ecrit les contraintes de check.
    /// </summary>
    /// <param name="writer">Flux crebas.</param>
    /// <param name="properties">Liste des propriétés persistantes.</param>
    private void WriteCheckConstraints(IFileWriter writer, IList<IProperty> properties)
    {
        WriteBooleanCheckConstraints(writer, properties);
    }

    /// <summary>
    /// Ajoute la fin de la déclaration de la table.
    /// </summary>
    /// <param name="writer">Flux d'écriture crebas.</param>
    private void WriteEndTableDeclaration(IFileWriter writer)
    {
        writer.WriteLine($"){GetTableTablespaceDeclaration()}{Config.BatchSeparator}");
    }

    /// <summary>
    /// Ajoute les contraintes de clés primaires.
    /// </summary>
    /// <param name="writer">Writer.</param>
    /// <param name="classe">Classe.</param>
    private void WritePrimaryKeyConstraint(IFileWriter writer, Class classe, IList<IProperty> properties)
    {
        if (!properties.Any(p => p.PrimaryKey))
        {
            return;
        }

        writer.Write("\tconstraint " + "PK_" + classe.SqlName + " primary key ");
        if (SupportsClusteredKey)
        {
            writer.Write("clustered ");
        }

        writer.WriteLine($"({string.Join(",", properties.Where(p => p.PrimaryKey).Select(pk => pk.SqlName))})");
    }

    private void WriteResourceTableDeclaration(IFileWriter writer)
    {
        if (Config.ResourcesTableName != null)
        {
            var tableName = Config.ResourcesTableName;
            writer.WriteLine("/**");
            writer.WriteLine("  * Création de ta table " + tableName + " contenant les traductions");
            writer.WriteLine(" **/");
            writer.WriteLine($"create table {Config.ResourcesTableName} (");
            writer.WriteLine(1, "RESOURCE_KEY varchar(255),");
            var hasLocale = translationStore.Translations.Keys.Count > 1 || translationStore.Translations.Keys.Any(a => a != string.Empty);
            if (hasLocale)
            {
                writer.WriteLine(1, "LOCALE varchar(10),");
            }

            writer.WriteLine(1, "LABEL varchar(4000),");
            writer.WriteLine(1, $"constraint PK_{Config.ResourcesTableName.ToConstantCase()} primary key (RESOURCE_KEY, LOCALE)");
            writer.WriteLine($"){Config.BatchSeparator}");

            writer.WriteLine("/**");
            writer.WriteLine("  * Création de l'index pour " + tableName + " (RESOURCE_KEY, LOCALE)");
            writer.WriteLine(" **/");
            writer.WriteLine("create index " + $"IDX_{Config.ResourcesTableName}_RESOURCE_KEY{(hasLocale ? "_LOCALE" : string.Empty)}" + " on " + tableName + " (");
            writer.WriteLine("\t" + $"RESOURCE_KEY{(hasLocale ? ", LOCALE" : string.Empty)}" + " ASC");
            writer.WriteLine($"){Config.BatchSeparator}");
            writer.WriteLine();
        }
    }

    private void WriteSequence(Class classe, IFileWriter writer, string tableName)
    {
        writer.WriteLine("/**");
        writer.WriteLine($"  * Création de la séquence pour la clé primaire de la table {tableName}");
        writer.WriteLine(" **/");

        WriteSequenceDeclaration(classe, writer, tableName);

        writer.WriteLine(Config.BatchSeparator);
        writer.WriteLine();
    }

    private List<AssociationProperty> WriteTableDeclaration(Class classe, IFileWriter writer)
    {
        var fkPropertiesList = new List<AssociationProperty>();

        var tableName = CheckIdentifierLength(classe.SqlName);

        writer.WriteLine("/**");
        writer.WriteLine("  * Création de la table " + tableName);
        writer.WriteLine(" **/");
        writer.WriteLine("create table " + tableName + " (");

        var properties = classe.GetAllProperties(Classes);

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

            writer.Write("\t" + CheckIdentifierLength(property.SqlName) + " " + persistentType);
            if (property is not AssociationProperty && property.PrimaryKey && property.Domain.AutoGeneratedValue && persistentType.Contains("int") && Config.Procedural!.Identity.Mode == IdentityMode.IDENTITY)
            {
                WriteIdentityColumn(writer);
            }

            if (property.Required)
            {
                writer.Write(" not null");
            }

            var defaultValue = Config.GetValue(property, Classes);
            if (defaultValue != "null")
            {
                writer.Write($" default {defaultValue}");
            }

            writer.Write(",");
            writer.WriteLine();

            if (property is AssociationProperty { Association.IsPersistent: true } ap)
            {
                fkPropertiesList.Add(ap);
            }
        }

        WriteCheckConstraints(writer, properties);
        WritePrimaryKeyConstraint(writer, classe, properties);
        WriteEndTableDeclaration(writer);

        writer.WriteLine();

        var shouldWriteSequence = Config.Procedural!.Identity.Mode == IdentityMode.SEQUENCE && classe.PrimaryKey.Count() == 1 && classe.PrimaryKey.Single().Domain.AutoGeneratedValue && !Config.GetType(classe.PrimaryKey.Single()).Contains("varchar", StringComparison.CurrentCultureIgnoreCase);
        if (shouldWriteSequence)
        {
            WriteSequence(classe, writer, tableName);
        }

        return fkPropertiesList;
    }
}
