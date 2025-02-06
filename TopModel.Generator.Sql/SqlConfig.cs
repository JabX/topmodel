using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Generator.Sql.Procedural;
using TopModel.Generator.Sql.Ssdt;

namespace TopModel.Generator.Sql;

public class SqlConfig : GeneratorConfigBase
{
    /// <summary>
    /// Config pour la génération en mode procédural.
    /// </summary>
    public ProceduralSqlConfig? Procedural { get; set; }

    /// <summary>
    /// Config pour la génération en mode SSDT.
    /// </summary>
    public SsdtConfig? Ssdt { get; set; }

    public override Dictionary<string, List<string>> TemplateAttributes => new()
    {
        { nameof(ForeignKeyConstraintNamePattern), new() { "tableName", "trigram", "columnName" } },
        { nameof(UniqueConstraintNamePattern), new() { "tableName", "columnNames", "propertyNames" } }
    };

    /// <summary>
    /// Désactive la génération des valeurs par défaut des propriétés dans les classes et endpoints générés avec cette configuration.
    /// </summary>
    public override bool IgnoreDefaultValues { get; set; } = true;

    /// <summary>
    /// Retourne ou définit le nom de la table contenant les traductions.
    /// </summary>
    public string? ResourcesTableName { get; set; }

    /// <summary>
    /// Retourne ou définit le nom du tablespace pour les tables (Postgres ou Oracle).
    /// </summary>
    public string? TableTablespace { get; set; }

    /// <summary>
    /// Retourne ou définit le nom du tablespace pour les index (Postgres ou Oracle).
    /// </summary>
    public string? IndexTablespace { get; set; }

    /// <summary>
    /// Retourne ou définit le pattern pour le nom des contraintes de clé étrangère.
    /// Supporte les variables tableName, trigram, columnName.
    /// Valeur par défaut : "FK_{tableName}_{columnName}".
    /// </summary>
    public string ForeignKeyConstraintNamePattern { get; set; } = "FK_{tableName}_{columnName}";

    /// <summary>
    /// Retourne ou définit le pattern pour le nom des contraintes d'unicité.
    /// Supporte les variables tableName, columnNames (avec trigramme), propertyNames (sans le trigramme).
    /// Valeur par défaut : "UK_{tableName}_{columnNames}".
    /// </summary>
    public string UniqueConstraintNamePattern { get; set; } = "UK_{tableName}_{columnNames}";

    /// <summary>
    /// SGBD cible ("sqlserver" ou "postgres" ou "oracle").
    /// </summary>
    public TargetDBMS TargetDBMS { get; set; } = TargetDBMS.Postgre;

    /// <summary>
    /// Indique si le SGBD gère les tablespaces.
    /// </summary>
    public bool AllowTablespace => TargetDBMS != TargetDBMS.Sqlserver;

    public string BatchSeparator => TargetDBMS switch
    {
        TargetDBMS.Oracle => $"{Environment.NewLine}/",
        TargetDBMS.Sqlserver => $"{Environment.NewLine}go",
        _ => ";"
    };

    protected override bool UseNamedEnums => false;

    public static bool IsBoolean(IProperty property)
    {
        var domain = property.Domain;
        /* Pour savoir si un domaine est booléen, on regarde grossièrement le mot bool dans le nom du domaine, le label du domaine, un type d'implémeentation de domaine. */
        return
            domain.Name.Value.Contains("bool", StringComparison.InvariantCultureIgnoreCase) ||
            domain.Label.Contains("bool", StringComparison.InvariantCultureIgnoreCase) ||
            domain.Implementations.Values.Any(di => di.Type?.Contains("bool", StringComparison.InvariantCultureIgnoreCase) ?? false);
    }

    public override bool CanClassUseEnums(Class classe, IEnumerable<Class>? availableClasses = null, IProperty? prop = null)
    {
        return false;
    }

    public string GetForeignKeyConstraintName(string tableName, string trigram, string columnName)
    {
        return ReplaceCustomVariables(
            ForeignKeyConstraintNamePattern,
            new Dictionary<string, string>
            {
                [nameof(tableName)] = tableName,
                [nameof(trigram)] = trigram,
                [nameof(columnName)] = columnName,
            });
    }

    /// <summary>
    /// Calcule le nom de la séquence associée à une table.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <returns>Nom de la séquence.</returns>
    public string GetSequenceName(Class classe)
    {
        return TargetDBMS switch
        {
            TargetDBMS.Oracle => $"{classe.Trigram}_SEQ",
            TargetDBMS.Postgre => $"SEQ_{classe.SqlName}",
            var t => throw new NotImplementedException($"Sequence declaration is not implemented with {t}")
        };
    }

    public string GetUniqueConstraintName(string tableName, string columnNames, string propertyNames)
    {
        return ReplaceCustomVariables(
            UniqueConstraintNamePattern,
            new Dictionary<string, string>
            {
                [nameof(tableName)] = tableName,
                [nameof(columnNames)] = columnNames,
                [nameof(propertyNames)] = propertyNames,
            });
    }

    public override string GetValue(IProperty property, IEnumerable<Class> availableClasses, string? value = null)
    {
        /* Cas spécifique d'un booléen sous Oracle, typé comme un numeric(1) */
        bool NeedsBooleanConversionToNumeric()
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return
                TargetDBMS == TargetDBMS.Oracle &&
                GetType(property) == "number(1)" &&
                IsBoolean(property);
        }

        if (NeedsBooleanConversionToNumeric())
        {
            return bool.Parse(value!) ? "1" : "0";
        }

        return base.GetValue(property, availableClasses, value);
    }

    public override bool ShouldQuoteValue(IProperty prop)
    {
        var type = GetType(prop);
        return (type ?? string.Empty).Contains("varchar")
            || type == "text"
            || type == "uniqueidentifier"
            || type == "uuid"
            || type == "bit"
            || (type ?? string.Empty).Contains("date")
            || (type ?? string.Empty).Contains("time");
    }

    protected override string GetEnumType(string className, string propName, bool isPrimaryKeyDef = false)
    {
        throw new NotImplementedException();
    }

    protected override string QuoteValue(string value)
    {
        return $@"{(TargetDBMS == TargetDBMS.Sqlserver ? "N" : string.Empty)}'{value.Replace("'", "''")}'";
    }

    /// <summary>
    /// Remplace des variables dans une chaîne.
    /// </summary>
    /// <param name="value">Chaîne templatisé sous la forme de {paramName}.</param>
    /// <param name="variables">Association entre paramName et paramValue.</param>
    /// <returns>Résultat.</returns>
    private static string ReplaceCustomVariables(string value, Dictionary<string, string> variables)
    {
        var buffer = value;
        foreach (var paramName in variables.Keys)
        {
            buffer = buffer.Replace($"{{{paramName}}}", variables[paramName]);
        }

        return buffer;
    }
}
