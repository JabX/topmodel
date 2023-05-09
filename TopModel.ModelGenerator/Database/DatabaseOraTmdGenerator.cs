﻿using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace TopModel.ModelGenerator.Database;

public class DatabaseOraTmdGenerator : DatabaseTmdGenerator
{
    private readonly DatabaseConfig _config;

#pragma warning disable CS8618
    public DatabaseOraTmdGenerator(ILogger<DatabaseOraTmdGenerator> logger, DatabaseConfig config)
        : base(logger, config, new OracleConnection(config.ConnectionString))
    {
        _config = config;
    }
#pragma warning restore CS8618

    public override string Name => "DatabaseOraGen";

    protected override string GetColumnsQuery()
    {
        return @$"
            select                 
                table_name                        AS TableName,
                column_name                       AS ColumnName,
                data_type                         AS DataType,
                nullable                          AS Nullable,    
                data_precision                    AS ""Precision"",
                coalesce(char_length, data_scale) AS Scale
            from all_tab_columns
            where owner = '{_config.Source.Schema}'
            order by table_name, column_id
            ";
    }

    protected override string GetForeignKeysQuery()
    {
        return @$"
                Select 
                    uc.constraint_name      AS ""Name"",
                    uc.table_name           AS TableName,
                    cols.column_name        AS ColumnName,
                    uc_pk.table_name        AS ForeignTableName,
                    cols_pk.column_name     AS ForeignColumnName
                From 
                    user_constraints uc
                    JOIN user_cons_columns cols ON uc.constraint_name = cols.constraint_name
                    JOIN user_constraints uc_pk ON uc.r_constraint_name = uc_pk.constraint_name
                    JOIN user_cons_columns cols_pk ON uc_pk.constraint_name = cols_pk.constraint_name
                Where 
                    uc.owner = '{_config.Source.Schema}'
                    and uc.constraint_type = 'R'
                Order by
                    uc.table_name,
                    cols.position
            ";
    }

    protected override string GetPrimaryKeysQuery()
    {
        return GetConstraintKeyQuery("P");
    }

    protected override string GetUniqueKeysQuery()
    {
        return GetConstraintKeyQuery("U");
    }

    private string GetConstraintKeyQuery(string name)
    {
        return @$"
                select 
                    uc.constraint_name AS ""Name"",
                    uc.table_name      AS TableName,
                    cols.column_name   AS ColumnName
                from 
                    user_constraints uc
                    join user_cons_columns cols ON uc.constraint_name = cols.constraint_name
                where
                    uc.owner = '{_config.Source.Schema}'
                    and uc.constraint_type = '{name}'
                order by
                    uc.table_name, 
                    cols.position
            ";
    }
}