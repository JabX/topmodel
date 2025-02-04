using System.Data.Common;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using TopModel.Utils;

namespace TopModel.ModelGenerator.Database;

public class DatabaseMySqlTmdGenerator(ILogger<DatabaseTmdGenerator> logger, DatabaseConfig config, GeneratedFileWriterProvider writerProvider)
    : DatabaseTmdGenerator(logger, config, writerProvider), IDisposable
{
    private readonly DatabaseConfig _config = config;

    public override string Name => "DatabaseMySqlGen";

    protected override string GetColumnsQuery()
    {
        // Récupération des colonnes
        return @$"
                select 
                    TABLE_NAME 											as TableName,
                    COLUMN_NAME 										as ColumnName,
                    DATA_TYPE 											as DataType,
                    case when IS_NULLABLE = 'NO' then 0 else 1 end 		as Nullable,
                    coalesce(NUMERIC_PRECISION, DATETIME_PRECISION) 	as 'Precision',
                    coalesce(CHARACTER_MAXIMUM_LENGTH, NUMERIC_SCALE) 	as Scale
                from information_schema.columns
                where table_schema = '{_config.Source.Schema}'
                order by table_name,ordinal_position
            ";
    }

    protected override DbConnection GetConnection()
    {
        return new MySqlConnection(_config.ConnectionString);
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
                    tc.TABLE_NAME as TableName,
                    kcu.COLUMN_NAME as ColumnName,
                    kcu.REFERENCED_TABLE_NAME as ForeignTableName,
                    kcu.REFERENCED_COLUMN_NAME as ForeignColumnName
                FROM 
                            information_schema.table_constraints   AS tc 
                    JOIN    information_schema.key_column_usage    AS kcu
                        ON  tc.constraint_name   = kcu.constraint_name
                        AND tc.table_schema      = kcu.table_schema
                        AND tc.TABLE_NAME  		 = kcu.TABLE_NAME
                WHERE       tc.constraint_type   = '{name}'
                    AND     tc.table_schema      = '{_config.Source.Schema}'
            ";
    }
}