using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural;

public class SqlCommentGenerator(ILogger<SqlCommentGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    public override string Name => "SqlCommentGen";

    protected override bool PersistentOnly => true;

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
            yield return ("comment", Config.Procedural!.CommentFile!);
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writer = this.OpenSqlWriter(fileName);

        var appName = classes.First().Namespace.App;

        writer.WriteLine("-- =========================================================================================== ");
        writer.WriteLine($"--   Application Name	:	{appName} ");
        writer.WriteLine("--   Script Name		:	" + fileName?.Split('/').Last());
        writer.WriteLine("--   Description		:	Script de création des commentaires. ");
        writer.WriteLine("-- =========================================================================================== ");

        foreach (var classe in classes.OrderBy(c => c.SqlName))
        {
            WriteTableDeclaration(classe, writer);
        }
    }

    protected void WriteComments(IFileWriter writer, Class classe, string tableName, IList<IProperty> properties)
    {
        writer.WriteLine();
        writer.WriteLine("/**");
        writer.WriteLine("  * Commentaires pour la table " + tableName);
        writer.WriteLine(" **/");
        writer.WriteLine($"COMMENT ON TABLE {tableName} IS '{classe.Comment.Replace("'", "''")}'{Config.BatchSeparator}");

        foreach (var p in properties)
        {
            writer.WriteLine($"COMMENT ON COLUMN {tableName}.{p.SqlName} IS '{p.Comment.Replace("'", "''")}'{Config.BatchSeparator}");
        }
    }

    /// <summary>
    /// Déclaration de la table.
    /// </summary>
    /// <param name="classe">La table à ecrire.</param>
    /// <param name="writerComment">Flux d'écritures des commentaires.</param>
    private void WriteTableDeclaration(Class classe, IFileWriter writerComment)
    {
        var tableName = CheckIdentifierLength(classe.SqlName);
        var properties = classe.GetAllProperties(Classes);
        WriteComments(writerComment, classe, tableName, properties);
    }
}
