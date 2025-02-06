using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.SqlServer;

public class SqlServerCrebasGenerator(ILogger<SqlServerCrebasGenerator> logger, TranslationStore translationStore, IFileWriterProvider writerProvider)
    : AbstractCrebasGenerator(logger, translationStore, writerProvider)
{
    public override string Name => "SqlServerCrebasGen";

    protected override bool SupportsClusteredKey => true;

    /// <summary>
    /// Gère l'auto-incrémentation des clés primaires en ajoutant identity à la colonne.
    /// </summary>
    /// <param name="writerCrebas">Flux d'écriture création bases.</param>
    protected override void WriteIdentityColumn(IFileWriter writerCrebas)
    {
        writerCrebas.Write(" identity(1, 1)");
    }
}
