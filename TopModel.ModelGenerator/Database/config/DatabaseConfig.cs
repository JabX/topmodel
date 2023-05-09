﻿namespace TopModel.ModelGenerator.Database;

public class DatabaseConfig
{

    public string OutputDirectory { get; set; } = "./";

    public IList<DomainMapping> Domains { get; set; } = new List<DomainMapping>();

    public DatabaseSource Source { get; set; } = new();

    public List<string> Exclude { get; set; } = new();

    public List<string> Tags { get; set; } = new();

    public List<string> ExtractValues { get; set; } = new();

    public List<ModuleConfig> Modules { get; set; } = new();

    public string ConnectionString => Source.DbType == DbType.POSTGRESQL ? PgConnectionString : OracleConnectionString;

    private string OracleConnectionString => $@"DATA SOURCE={Source.Host}:{Source.Port}/{Source.DbName};USER ID={Source.User};password={Source.Password}";

    private string PgConnectionString => @$"Host={Source.Host};Port={Source.Port};Database={Source.DbName};Username={Source.User}{(Source.Password != null ? $";Password={Source.Password}" : string.Empty)}";
}
