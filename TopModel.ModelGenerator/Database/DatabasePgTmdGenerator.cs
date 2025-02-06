using System.Data.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using TopModel.Utils;

namespace TopModel.ModelGenerator.Database;

public class DatabasePgTmdGenerator(ILogger<DatabaseTmdGenerator> logger, DatabaseConfig config, IFileWriterProvider writerProvider)
    : DatabaseTmdGenerator(logger, config, writerProvider), IDisposable
{
    private readonly DatabaseConfig _config = config;

    public override string Name => "DatabasePgGen";

    protected override string GetColumnsQuery()
    {
        // Récupération des colonnes
        return @$"
                select  table_name                                                              as TableName, 
                        column_name                                                             as ColumnName, 
                        data_type                                                               as DataType,
                        is_nullable = 'YES'                                                     as Nullable,
                        coalesce(numeric_precision, datetime_precision, interval_precision)     as Precision,
                        coalesce(character_maximum_length, numeric_scale, interval_precision)   as Scale
                from information_schema.columns 
                where table_schema  = '{_config.Source.Schema}'
                order by ordinal_position 
            ";
    }

    protected override DbConnection GetConnection()
    {
        return new NpgsqlConnection(_config.ConnectionString);
    }

    protected override string GetForeignKeysQuery()
    {
        // Récupération des contraintes de clés étrangères
        return GetConstraintKeyQuery("FOREIGN KEY");
    }

    protected override string GetPrimaryKeysQuery()
    {
        // Récupération des contraintes de clés primaires
        return GetConstraintKeyQuery("PRIMARY KEY");
    }

    protected override string GetUniqueKeysQuery()
    {
        // Récupération des contraintes d'unicité
        return $@"
                SELECT
                    tc.constraint_name  AS Name,
                    tc.table_name       AS TableName,
                    kcu.column_name     AS ColumnName
                FROM 
                    information_schema.table_constraints        AS tc 
                    JOIN information_schema.key_column_usage    AS kcu
                        ON  tc.constraint_name      = kcu.constraint_name
                        AND tc.table_schema         = kcu.table_schema
                WHERE       tc.constraint_type      = 'UNIQUE'
                    AND     tc.table_schema         = '{_config.Source.Schema}'
            ";
    }

    private string GetConstraintKeyQuery(string name)
    {
        return @$"
                SELECT
                    tc.table_name   AS TableName, 
                    kcu.column_name AS ColumnName, 
                    ccu.table_name  AS ForeignTableName,
                    ccu.column_name AS ForeignColumnName 
                FROM 
                            information_schema.table_constraints   AS tc 
                    JOIN    information_schema.key_column_usage    AS kcu
                        ON  tc.constraint_name   = kcu.constraint_name
                        AND tc.table_schema      = kcu.table_schema
                    JOIN information_schema.constraint_column_usage AS ccu
                        ON  ccu.constraint_name  = tc.constraint_name
                        AND ccu.table_schema     = tc.table_schema
                WHERE       tc.constraint_type   = '{name}'
                    AND     tc.table_schema      = '{_config.Source.Schema}'
                    AND     ccu.table_schema     = '{_config.Source.Schema}'
            ";
    }
}