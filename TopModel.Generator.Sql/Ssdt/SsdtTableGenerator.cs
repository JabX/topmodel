using System.Text;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Ssdt;

/// <summary>
/// Scripter permettant d'écrire les scripts de création d'une table SQL avec :
/// - sa structure
/// - sa contrainte PK
/// - ses contraintes FK
/// - ses indexes FK
/// - ses contraintes d'unicité sur colonne unique.
/// </summary>
public class SsdtTableGenerator(ILogger<SsdtTableGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGeneratorBase<SqlConfig>(logger, writerProvider)
{
    public override string Name => "SsdtTableGen";

    protected override bool PersistentOnly => true;

    protected override bool FilterClass(Class classe)
    {
        return classe.IsPersistent && !classe.Abstract;
    }

    protected override IEnumerable<Class> GetExtraClasses(ModelFile file)
    {
        return file.GetExtraClasses();
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Path.Combine(Config.Ssdt!.TableScriptFolder!, classe.SqlName + ".sql");
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        using var writer = this.OpenFileWriter(fileName);

        // TODO : rendre paramétrable.
        var useCompression = false;

        // Entête du fichier.
        WriteHeader(writer, classe.SqlName);

        // Ouverture du create table.
        WriteCreateTableOpening(writer, classe);

        // Intérieur du create table.
        var properties = WriteInsideInstructions(writer, classe);

        // Fin du create table.
        WriteCreateTableClosing(writer, classe, useCompression);

        // Indexes sur les clés étrangères.
        GenerateIndexForeignKey(writer, classe.SqlName, properties);

        // Définition
        if (Config.TargetDBMS == TargetDBMS.Sqlserver)
        {
            WriteTableDescriptionProperty(writer, classe);
        }
    }

    /// <summary>
    /// Ecrit l'entête du fichier.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="tableName">Nom de la table.</param>
    private static void WriteHeader(IFileWriter writer, string tableName)
    {
        writer.WriteLine("-- ===========================================================================================");
        writer.WriteLine("--   Description		:	Création de la table " + tableName + ".");
        writer.WriteLine("-- ===========================================================================================");
        writer.WriteLine();
    }

    /// <summary>
    /// Ecrit la création de la propriété de description de la table.
    /// </summary>
    /// <param name="writer">Writer.</param>
    /// <param name="classe">Classe de la table.</param>
    private static void WriteTableDescriptionProperty(IFileWriter writer, Class classe)
    {
        writer.WriteLine("/* Description property. */");
        writer.WriteLine("EXECUTE sp_addextendedproperty 'Description', '" + classe.Label?.Replace("'", "''") + "', 'SCHEMA', 'dbo', 'TABLE', '" + classe.SqlName + "';");
    }

    /// <summary>
    /// Génère les indexes portant sur les FK.
    /// </summary>
    /// <param name="writer">Flux d'écriture.</param>
    /// <param name="tableName">Nom de la table.</param>
    /// <param name="properties">Champs.</param>
    private void GenerateIndexForeignKey(IFileWriter writer, string tableName, IList<IProperty> properties)
    {
        var fkList = properties.OfType<AssociationProperty>().ToList();
        foreach (var property in fkList)
        {
            var propertyName = ((IProperty)property).SqlName;
            var indexName = "IDX_" + tableName + "_" + propertyName + "_FK";

            writer.WriteLine("/* Index on foreign key column for " + tableName + "." + propertyName + " */");

            if (Config.TargetDBMS == TargetDBMS.Sqlserver)
            {
                writer.WriteLine("create nonclustered index [" + indexName + "]");
                writer.Write("\ton [dbo].[" + tableName + "] (");
                var propertyConcat = "[" + propertyName + "] ASC";

                writer.Write(propertyConcat);
                writer.Write(")");

                writer.WriteLine();
                writer.WriteLine("go");
            }
            else
            {
                writer.WriteLine($"create index {indexName}");
                writer.Write($"\ton {tableName} (");
                writer.Write(propertyName);
                writer.WriteLine(" asc);");
            }

            writer.WriteLine();
        }
    }

    /// <summary>
    /// Ecrit le SQL pour une colonne.
    /// </summary>
    /// <param name="sb">Flux.</param>
    /// <param name="property">Propriété.</param>
    private void WriteColumn(StringBuilder sb, IProperty property)
    {
        var persistentType = property is not CompositionProperty
            ? Config.GetType(property, Classes)
            : Config.TargetDBMS == TargetDBMS.Postgre ? "jsonb" : "json";

        if (Config.TargetDBMS == TargetDBMS.Sqlserver)
        {
            sb.Append('[');
        }

        sb.Append(property.SqlName);

        if (Config.TargetDBMS == TargetDBMS.Sqlserver)
        {
            sb.Append(']');
        }

        sb.Append($" {persistentType}");

        if (property is not AssociationProperty && property.PrimaryKey && property.Domain.AutoGeneratedValue && Config.GetType(property, Classes).Contains("int") && !Config.Ssdt!.DisableIdentity)
        {
            if (Config.TargetDBMS == TargetDBMS.Sqlserver)
            {
                sb.Append(" identity");
            }
            else
            {
                sb.Append(" not null generated always as identity");
            }
        }

        if (property.Required && !property.PrimaryKey)
        {
            sb.Append(" not null");
        }

        var defaultValue = Config.GetValue(property, Classes);
        if (defaultValue != "null")
        {
            sb.Append($" default {defaultValue}");
        }
    }

    /// <summary>
    /// Génère la contrainte de clef étrangère.
    /// </summary>
    /// <param name="sb">Flux d'écriture.</param>
    /// <param name="property">Propriété portant la clef étrangère.</param>
    private void WriteConstraintForeignKey(StringBuilder sb, AssociationProperty property)
    {
        var tableName = property.Class.SqlName;

        var propertyName = ((IProperty)property).SqlName;
        var referenceClass = property.Association;

        if (Config.TargetDBMS == TargetDBMS.Sqlserver)
        {
            var constraintName = "FK_" + tableName + "_" + referenceClass.SqlName + "_" + propertyName;
            var propertyConcat = "[" + propertyName + "]";
            sb.Append("constraint [").Append(constraintName).Append("] foreign key (").Append(propertyConcat).Append(") ");
            sb.Append("references [dbo].[").Append(referenceClass.SqlName).Append("] (");
            sb.Append('[').Append(property.Property.SqlName).Append(']');
            sb.Append(')');
        }
        else
        {
            sb.Append($"constraint FK_{tableName}_{referenceClass.SqlName}_{propertyName} foreign key ({propertyName}) references {referenceClass.SqlName} ({property.Property.SqlName})");
        }
    }

    /// <summary>
    /// Ecrit le pied du script.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="classe">Classe de la table.</param>
    /// <param name="useCompression">Indique si on utilise la compression.</param>
    private void WriteCreateTableClosing(IFileWriter writer, Class classe, bool useCompression)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(classe);

        writer.WriteLine(")");

        if (useCompression)
        {
            writer.WriteLine("WITH (DATA_COMPRESSION=PAGE)");
        }

        writer.WriteLine(Config.TargetDBMS == TargetDBMS.Sqlserver ? "go" : ";");
        writer.WriteLine();
    }

    /// <summary>
    /// Ecrit l'ouverture du create table.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="table">Table.</param>
    private void WriteCreateTableOpening(IFileWriter writer, Class table)
    {
        if (Config.TargetDBMS == TargetDBMS.Sqlserver)
        {
            writer.WriteLine($"create table [dbo].[{table.SqlName}] (");
        }
        else
        {
            writer.WriteLine($"create table {table.SqlName} (");
        }
    }

    /// <summary>
    /// Ecrit les instructions à l'intérieur du create table.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="table">Table.</param>
    private List<IProperty> WriteInsideInstructions(IFileWriter writer, Class table)
    {
        // Construction d'une liste de toutes les instructions.
        var definitions = new List<string>();
        var sb = new StringBuilder();

        // Colonnes
        var properties = table.Properties.Where(p => p is not AssociationProperty ap || ap.Type == AssociationType.ManyToOne || ap.Type == AssociationType.OneToOne).ToList();

        if (table.Extends != null)
        {
            properties.Add(new AssociationProperty
            {
                Association = table.Extends,
                Class = table,
                Required = true,
                PrimaryKey = !table.PrimaryKey.Any()
            });
        }

        var oneToManyProperties = Classes.SelectMany(cl => cl.Properties).OfType<AssociationProperty>().Where(ap => ap.Type == AssociationType.OneToMany && ap.Association == table);
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
            sb.Clear();
            WriteColumn(sb, property);
            definitions.Add(sb.ToString());
        }

        // Primary Key
        sb.Clear();
        WritePkLine(sb, table, properties);

        definitions.Add(sb.ToString());

        // Foreign key constraints
        foreach (var property in properties.OfType<AssociationProperty>().Where(ap => ap.Association.IsPersistent && Classes.Contains(ap.Association)))
        {
            sb.Clear();
            WriteConstraintForeignKey(sb, property);
            definitions.Add(sb.ToString());
        }

        // Unique constraints
        definitions.AddRange(WriteUniqueConstraints(table));

        // Ecriture de la liste concaténée.
        var separator = "," + Environment.NewLine;
        writer.Write(string.Join(separator, definitions.Select(x => "\t" + x)));

        return properties;
    }

    /// <summary>
    /// Ecrit la ligne de création de la PK.
    /// </summary>
    /// <param name="sb">Flux.</param>
    /// <param name="classe">Classe.</param>
    private void WritePkLine(StringBuilder sb, Class classe, List<IProperty> properties)
    {
        var pkCount = 0;

        if (!properties.Any(p => p.PrimaryKey))
        {
            return;
        }

        if (Config.TargetDBMS == TargetDBMS.Sqlserver)
        {
            sb.Append("constraint [PK_").Append(classe.SqlName).Append("] primary key clustered (");
        }
        else
        {
            sb.Append($"constraint PK_{classe.SqlName} primary key (");
        }

        foreach (var pk in properties.Where(p => p.PrimaryKey))
        {
            ++pkCount;
            if (Config.TargetDBMS == TargetDBMS.Sqlserver)
            {
                sb.Append($"[{pk.SqlName}] ASC");
            }
            else
            {
                sb.Append(pk.SqlName);
            }

            if (pkCount < properties.Count(p => p.PrimaryKey))
            {
                sb.Append(", ");
            }
        }

        sb.Append(')');
    }

    /// <summary>
    /// Calcule la liste des déclarations de contraintes d'unicité.
    /// </summary>
    /// <param name="classe">Classe de la table.</param>
    /// <returns>Liste des déclarations de contraintes d'unicité.</returns>
    private List<string> WriteUniqueConstraints(Class classe)
    {
        return classe.UniqueKeys
            .Concat(classe.Properties.OfType<AssociationProperty>().Where(ap => ap.Type == AssociationType.OneToOne).Select(ap => new List<IProperty> { ap }))
            .Select(uk => Config.TargetDBMS == TargetDBMS.Sqlserver
             ? $"constraint [UK_{classe.SqlName}_{string.Join("_", uk.Select(p => p.SqlName))}] unique nonclustered ({string.Join(", ", uk.Select(p => $"[{p.SqlName}] ASC"))})"
             : $"constraint UK_{classe.SqlName}_{string.Join("_", uk.Select(p => p.SqlName))} unique ({string.Join(", ", uk.Select(p => $"{p.SqlName}"))})")
            .ToList();
    }
}