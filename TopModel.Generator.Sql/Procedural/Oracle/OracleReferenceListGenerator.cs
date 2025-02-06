using Microsoft.Extensions.Logging;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.Oracle;

public class OracleReferenceListGenerator(ILogger<OracleReferenceListGenerator> logger, IFileWriterProvider writerProvider)
    : AbstractReferenceListGenerator(logger, writerProvider)
{
    public override string Name => "OracleRefListGen";

    protected override bool ExplicitSequenceNextVal => true;

    protected override string GetNextValCall(string sequenceName)
    {
        return $"{sequenceName}.nextval";
    }
}
