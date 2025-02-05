using System.Text;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Ssdt;

/// <summary>
/// Scripter permettant d'écrire les scripts de création d'un type de table SQL.
/// </summary>
public class SsdtTableTypeGenerator(ILogger<ClassGeneratorBase<SqlConfig>> logger, IFileWriterProvider writerProvider)
    : ClassGeneratorBase<SqlConfig>(logger, writerProvider)
{
    public override string Name => "SsdtTableTypeGen";

    protected override bool PersistentOnly => true;

    protected override bool FilterClass(Class classe)
    {
        return classe.IsPersistent && !classe.Abstract && classe.Properties.Any(p => p.Name == ScriptUtils.InsertKeyName);
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Path.Combine(Config.Ssdt!.TableTypeScriptFolder!, classe.GetTableTypeName() + ".sql");
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        using var writer = this.OpenSqlFileWriter(fileName);

        // Entête du fichier.
        WriteHeader(writer, classe.GetTableTypeName());

        // Ouverture du create table.
        WriteCreateTableOpening(writer, classe);

        // Intérieur du create table.
        WriteInsideInstructions(writer, classe);

        // Fin du create table.
        WriteCreateTableClosing(writer);
    }

    /// <summary>
    /// Ecrit le pied du script.
    /// </summary>
    /// <param name="writer">Flux.</param>
    private static void WriteCreateTableClosing(IFileWriter writer)
    {
        writer.WriteLine(")");
        writer.WriteLine("go");
        writer.WriteLine();
    }

    /// <summary>
    /// Ecrit l'ouverture du create table.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="table">Table.</param>
    private static void WriteCreateTableOpening(IFileWriter writer, Class table)
    {
        writer.WriteLine("Create type [" + table.GetTableTypeName() + "] as Table (");
    }

    /// <summary>
    /// Ecrit l'entête du fichier.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="tableName">Nom de la table.</param>
    private static void WriteHeader(IFileWriter writer, string tableName)
    {
        writer.WriteLine("-- ===========================================================================================");
        writer.WriteLine("--   Description		:	Création du type de table " + tableName + ".");
        writer.WriteLine("-- ===========================================================================================");
        writer.WriteLine();
    }

    /// <summary>
    /// Ecrit la colonne InsertKey.
    /// </summary>
    /// <param name="sb">Flux.</param>
    /// <param name="classe">Classe.</param>
    private static void WriteInsertKeyLine(StringBuilder sb, Class classe)
    {
        sb.Append('[').Append((classe.Trigram != null ? $"{classe.Trigram}_" : string.Empty) + "INSERT_KEY] int null");
    }

    /// <summary>
    /// Ecrit le SQL pour une colonne.
    /// </summary>
    /// <param name="sb">Flux.</param>
    /// <param name="property">Propriété.</param>
    private void WriteColumn(StringBuilder sb, IProperty property)
    {
        var persistentType = Config.GetType(property);
        sb.Append('[').Append(property.SqlName).Append("] ").Append(persistentType).Append(" null");
    }

    /// <summary>
    /// Ecrit les instructions à l'intérieur du create table.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="table">Table.</param>
    private void WriteInsideInstructions(IFileWriter writer, Class table)
    {
        // Construction d'une liste de toutes les instructions.
        var definitions = new List<string>();
        var sb = new StringBuilder();

        // Colonnes
        foreach (var property in table.Properties)
        {
            if ((!property.PrimaryKey || Config.ShouldQuoteValue(property)) && property.Name != ScriptUtils.InsertKeyName)
            {
                sb.Clear();
                WriteColumn(sb, property);
                definitions.Add(sb.ToString());
            }
        }

        // InsertKey.
        sb.Clear();
        WriteInsertKeyLine(sb, table);
        definitions.Add(sb.ToString());

        // Ecriture de la liste concaténée.
        var separator = "," + Environment.NewLine;
        writer.Write(string.Join(separator, definitions.Select(x => "\t" + x)));
    }
}