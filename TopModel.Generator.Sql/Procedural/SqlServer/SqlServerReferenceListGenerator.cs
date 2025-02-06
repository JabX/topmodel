using Microsoft.Extensions.Logging;
using TopModel.Utils;

namespace TopModel.Generator.Sql.Procedural.SqlServer;

public class SqlServerReferenceListGenerator(ILogger<SqlServerReferenceListGenerator> logger, IFileWriterProvider writerProvider)
    : AbstractReferenceListGenerator(logger, writerProvider)
{
    public override string Name => "SqlServerRefListGen";

    protected override void WriteInsertEnd(IFileWriter writerInsert)
    {
        writerInsert.WriteLine("set nocount off;");
        writerInsert.WriteLine();
    }

    protected override void WriteInsertStart(IFileWriter writerInsert)
    {
        writerInsert.WriteLine("set nocount on;");
        writerInsert.WriteLine();
    }
}
