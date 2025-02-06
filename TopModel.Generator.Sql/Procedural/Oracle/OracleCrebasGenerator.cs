using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.Oracle;

public class OracleCrebasGenerator(ILogger<OracleCrebasGenerator> logger, TranslationStore translationStore, IFileWriterProvider writerProvider)
    : AbstractCrebasGenerator(logger, translationStore, writerProvider)
{
    public override string Name => "OracleCrebasGen";

    protected override bool SupportsClusteredKey => false;

    protected override void WriteBooleanCheckConstraints(IFileWriter writer, IList<IProperty> properties)
    {
        /* En Oracle, en 2024, il n'y a pas de type booléen. On utilise un numeric(1) et on rajoute une check constraint pour forcer les valeurs 0 et 1. */
        bool IsNumericBoolean(IProperty property)
        {
            var sqlType = Config.GetType(property);
            return sqlType == "number(1)" && SqlConfig.IsBoolean(property);
        }

        foreach (var property in properties)
        {
            if (IsNumericBoolean(property))
            {
                writer.WriteLine("\tconstraint " + CheckIdentifierLength($"CHK_{property.SqlName}") + " check (" + property.SqlName + " in (0,1)),");
            }
        }
    }

    /// <summary>
    /// Gère l'auto-incrémentation des clés primaires en ajoutant identity à la colonne.
    /// </summary>
    /// <param name="writer">Flux d'écriture création bases.</param>
    protected override void WriteIdentityColumn(IFileWriter writer)
    {
        throw new NotImplementedException("Non implémenté");
    }

    protected override void WriteSequenceDeclaration(Class classe, IFileWriter writer, string tableName)
    {
        writer.Write($"create sequence {Config.GetSequenceName(classe)}");

        if (Config.Procedural!.Identity.Start != null)
        {
            writer.Write($"{$" start with {Config.Procedural!.Identity.Start}"}");
        }

        if (Config.Procedural!.Identity.Increment != null)
        {
            writer.Write($"{$" increment by {Config.Procedural!.Identity.Increment} nocycle"}");
        }
    }
}
