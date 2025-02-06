using Microsoft.Extensions.Logging;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.Postgres;

public class PostgresReferenceListGenerator(ILogger<PostgresReferenceListGenerator> logger, IFileWriterProvider writerProvider)
    : AbstractReferenceListGenerator(logger, writerProvider)
{
    public override string Name => "PostgresRefListGen";
}
