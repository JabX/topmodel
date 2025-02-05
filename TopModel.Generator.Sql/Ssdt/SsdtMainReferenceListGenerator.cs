using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Ssdt;

/// <summary>
/// Scripter écrivant un script qui ordonnance l'appel aux scripts d'insertions de valeurs de listes de références.
/// </summary>
public class SsdtMainReferenceListGenerator(ILogger<ClassGroupGeneratorBase<SqlConfig>> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<SqlConfig>(logger, writerProvider)
{
    public override string Name => "SsdtMainRefListGen";

    protected override bool PersistentOnly => true;

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsPersistent && !classe.Abstract && classe.Values.Count > 0)
        {
            yield return ("main", Path.Combine(Config.Ssdt!.InitListScriptFolder!, Config.Ssdt!.InitListMainScriptName!).Replace("\\", "/"));
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        using var writer = this.OpenSqlFileWriter(fileName);

        // Entête du fichier.
        WriteHeader(writer);

        // Construit la liste des Reference Class ordonnée.
        var orderList = CoreUtils.Sort(classes.OrderBy(c => c.SqlName), c => c.Properties
            .OfType<AssociationProperty>()
            .Select(a => a.Association)
            .Where(a => a != c && a.Values.Count > 0));

        // Appel des scripts d'insertion.
        WriteScriptCalls(writer, orderList);
    }

    /// <summary>
    /// Ecrit l'entête du fichier.
    /// </summary>
    /// <param name="writer">Flux.</param>
    private static void WriteHeader(IFileWriter writer)
    {
        writer.WriteLine("-- ===========================================================================================");
        writer.WriteLine("--   Description		:	Insertion des valeurs de listes statiques.");
        writer.WriteLine("-- ===========================================================================================");
        writer.WriteLine();
    }

    /// <summary>
    /// Ecrit les appels de scripts.
    /// </summary>
    /// <param name="writer">Flux.</param>
    /// <param name="classSet">Ensemble des listes de référence.</param>
    private static void WriteScriptCalls(IFileWriter writer, IEnumerable<Class> classSet)
    {
        foreach (var classe in classSet)
        {
            var subscriptName = classe.SqlName + ".insert.sql";
            writer.WriteLine("/* Insertion dans la table " + classe.SqlName + ". */");
            writer.WriteLine(":r .\\" + subscriptName);
            writer.WriteLine();
        }
    }
}