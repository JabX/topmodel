using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural;

public class SqlUkGenerator(ILogger<SqlUkGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    public override string Name => "SqlUkGen";

    protected override bool PersistentOnly => true;

    protected override IEnumerable<Class> GetExtraClasses(ModelFile file)
    {
        return file.GetExtraClasses();
    }

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract)
        {
            yield return ("uk", Config.Procedural!.UniqueKeysFile!);
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writer = this.OpenSqlWriter(fileName);

        var appName = classes.First().Namespace.App;

        writer.WriteLine("-- =========================================================================================== ");
        writer.WriteLine($"--   Application Name	:	{appName} ");
        writer.WriteLine("--   Script Name		:	" + fileName?.Split('/').Last());
        writer.WriteLine("--   Description		:	Script de création des contraintes d'unicité.");
        writer.WriteLine("-- =========================================================================================== ");

        foreach (var classe in classes.OrderBy(c => c.SqlName))
        {
            WriteUniqueKeys(classe, writer);
        }
    }

    /// <summary>
    /// Ajoute les contraintes d'unicité.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <param name="writer">Writer.</param>
    private void WriteUniqueKeys(Class classe, IFileWriter writer)
    {
        foreach (var uk in classe.UniqueKeys
            .Concat(classe.Properties.OfType<AssociationProperty>().Where(ap => ap.Type == AssociationType.OneToOne).Select(ap => new List<IProperty> { ap })))
        {
            string columnNames = string.Join("_", uk.Select(p => p.SqlName));
            string propertyNames = string.Join("_", uk.Select(p => GetPropertyName(p.SqlName)));
            string constraintName = Config.GetUniqueConstraintName(classe.SqlName, columnNames, propertyNames);
            writer?.WriteLine($"alter table {classe.SqlName} add constraint {constraintName} unique ({string.Join(", ", uk.Select(p => p.SqlName))}){Config.BatchSeparator}");
            writer?.WriteLine();
        }

        static string GetPropertyName(string columnName)
        {
            /* Retire le préfixe du trigram (TRI_CODE => CODE). */
            return columnName.Length > 4 ? columnName[4..] : columnName;
        }
    }
}
