using System.Data.Common;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Extensions.Logging;
using TopModel.Utils;

namespace TopModel.ModelGenerator.Database;

public abstract class DatabaseTmdGenerator(ILogger<DatabaseTmdGenerator> logger, DatabaseConfig config, GeneratedFileWriterProvider writerProvider)
    : ModelGenerator(logger), IDisposable
{
    private readonly Dictionary<string, TmdClass> _classes = [];
    private DbConnection? _connection;

    private int _fileIndice = 10;
    private int _moduleIndice = 0;

    public required Dictionary<string, string> Passwords { get; init; }

    public override string Name => "DatabaseGen";

    private IEnumerable<TmdFile> Files => _classes.Select(c => c.Value.File!).Distinct().OrderBy(c => c.Name);

    private string ModuleIndice
    {
        get
        {
            _moduleIndice++;
            return _moduleIndice.ToString().PadLeft(2, '0');
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose" />
    public void Dispose()
    {
        _connection?.Dispose();
    }

    protected override async IAsyncEnumerable<string> GenerateCore()
    {
        InitConnection();
        logger.LogInformation($"Connexion à la base de données {config.Source.DbName} réussie !");
        logger.LogInformation($"Génération en cours, veuillez patienter...");
        var columns = await GetColumns();
        var classGroups = columns.GroupBy(c => c.TableName);

        // Initialisation de {classGroups.Count()} classes
        InitClasses(classGroups);

        // Initialisation des propriétés
        await InitProperties(classGroups);
        await InitUniqConstraints(classGroups);
        await ReadValues();

        CreateFiles();

        // Création des modules
        CreateModules();

        // Regroupement des fichiers qui ont les mêmes imports
        GroupFiles();

        // Renumérotation et réétiquettage des fichiers
        RenameFiles();

        // Ecriture des fichiers
        foreach (var file in WriteFiles())
        {
            yield return file;
        }
    }

    protected abstract string GetColumnsQuery();

    protected abstract DbConnection GetConnection();

    protected abstract string GetForeignKeysQuery();

    protected abstract string GetPrimaryKeysQuery();

    protected abstract string GetUniqueKeysQuery();

    private static void AddUniqConstraints(TmdClass classe, IEnumerable<IGrouping<string, ConstraintKey>> groupings)
    {
        foreach (var group in groupings)
        {
            var uniqConstraints = group.GroupBy(g => g.Name);
            classe.Unique
                .AddRange(uniqConstraints.Select(u => u.Where(c => classe.Properties.OfType<TmdRegularProperty>().Any(p => p.SqlName == c.ColumnName)).Select(c =>
                {
                    var property = classe.Properties.OfType<TmdRegularProperty>().First(p => p.SqlName == c.ColumnName);
                    string name = string.Empty;
                    if (property is TmdAssociationProperty ap)
                    {
                        name = ap.Association.Name + ap.ForeignProperty!.Name + ap.Role;
                    }
                    else
                    {
                        name = property.Name;
                    }

                    return name;
                }).Distinct().ToList()));
        }
    }

    private void AffectModule(TmdClass mainClass, string? module = null)
    {
        var rootFile = mainClass.File!;
        module ??= $"{ModuleIndice}_{mainClass.Name}";

        var stack = new Queue<TmdFile>();
        stack.Enqueue(rootFile);
        while (stack.TryDequeue(out var f))
        {
            f.Module = module;

            // Ajout des classes dont je dépends
            foreach (var d in f.Classes.SelectMany(c => c.Dependencies.Select(d => d.File!)).Distinct().Where(d => d.Module is null))
            {
                stack.Enqueue(d);
            }
        }
    }

    private TmdProperty ColumnToProperty(TmdClass classe, DbColumn column, string trigram, ConstraintKey? primaryKeyConstraint, ConstraintKey? foreignConstraint)
    {
        var domain = TmdGenUtils.GetDomainString(config.Domains, name: column.ColumnName, scale: column.Scale, precision: column.Precision);
        if (domain == column.ColumnName)
        {
            domain = TmdGenUtils.GetDomainString(config.Domains, type: column.DataType, scale: column.Scale, precision: column.Precision);
        }

        var columnName = column.ColumnName;

        TmdRegularProperty tmdProperty;
        if (foreignConstraint != null && _classes.TryGetValue(foreignConstraint.ForeignTableName, out var foreignClass))
        {
            tmdProperty = new TmdAssociationProperty();
            ((TmdAssociationProperty)tmdProperty).Association = foreignClass;
        }
        else
        {
            tmdProperty = new TmdRegularProperty();
            if (!string.IsNullOrEmpty(trigram))
            {
                var regex = new Regex(Regex.Escape(trigram));
                columnName = regex.Replace(columnName, string.Empty, 1);
            }
        }

        tmdProperty.Name = columnName.ToLower().ToPascalCase();
        tmdProperty.Required = !column.Nullable || primaryKeyConstraint != null;
        tmdProperty.PrimaryKey = primaryKeyConstraint != null;
        tmdProperty.Domain = domain;
        tmdProperty.SqlName = column.ColumnName;
        tmdProperty.Class = classe;

        return tmdProperty;
    }

    private void CreateAutomaticModules()
    {
        while (Files.Any(f => f.Module is null && f.Classes.SelectMany(c => c.Dependencies).Count() > 2))
        {
            var rootFile = Files.Where(f => f.Module is null && f.Classes.SelectMany(c => c.Dependencies).Count() > 2)
                .OrderByDescending(f => f.Classes.Select(c => c.Dependencies.Count + _classes.Select(cl => cl.Value.Dependencies.Contains(c)).Count()).Sum())
                .First();
            var mainClass = rootFile.Classes.OrderByDescending(cl => cl.Dependencies.Count + _classes.Select(c => c.Value.Dependencies.Contains(cl)).Count()).First();

            AffectModule(mainClass);
        }
    }

    private void CreateCircularFiles()
    {
        while (_classes.Any(c => c.Value.File == null))
        {
            var file = new TmdFile()
            {
                Name = $"{_fileIndice++}_Model",
                Tags = config.Tags
            };
            var rootClass = _classes.Where(c => c.Value.File == null).First();
            var stack = new Queue<TmdClass>();
            stack.Enqueue(rootClass.Value);
            while (stack.TryDequeue(out var s))
            {
                s.File = file;
                file.Classes.Add(s);
                foreach (var d in s.Dependencies.Where(d => d.File is null))
                {
                    stack.Enqueue(d);
                }
            }
        }
    }

    private void CreateFiles()
    {
        // Création des fichiers de classes sans dépendances cirulaires
        CreateSimpleFiles();

        // Création des fichiers des classes contenant des dépendances cirulaires
        CreateCircularFiles();
    }

    private void CreateJoinModule()
    {
        var joinModule = $"{ModuleIndice}_Join";
        foreach (var file in Files.Where(f => f.Module is null && f.Classes.SelectMany(c => c.Dependencies).Any()))
        {
            var dep = file.Classes.SelectMany(c => c.Dependencies).Select(c => c.File!.Module).Distinct();
            if (dep.Count() == 1 && dep.First() != null)
            {
                file.Module = dep.First();
            }
            else
            {
                file.Module = joinModule;
            }
        }
    }

    private void CreateModules()
    {
        // Regroupement des classes dans les modules définis dans la configuration
        CreateUserModules();

        // Recherche automatique des modules restants (classes ayant plus de deux dépendances)
        CreateAutomaticModules();

        // Regroupement des classes qui ont une ou deux dépendances
        CreateJoinModule();

        // Regroupement des classes qui n'ont aucune dépendance
        CreateOtherModule();
    }

    private void CreateOtherModule()
    {
        var trashModule = $"{ModuleIndice}_Autres";
        foreach (var file in Files.Where(f => f.Module is null))
        {
            file.Module = trashModule;
        }
    }

    private void CreateSimpleFiles()
    {
        while (_classes.Where(c => c.Value.File == null && c.Value.Dependencies.All(d => d.File != null)).Any())
        {
            foreach (var classe in _classes.Where(c => c.Value.File == null && c.Value.Dependencies.All(d => d.File != null)).OrderBy(c => c.Value.Dependencies.Count))
            {
                var file = new TmdFile()
                {
                    Name = $"{_fileIndice++}_Model",
                    Tags = config.Tags
                };
                classe.Value.File = file;
                file.Classes.Add(classe.Value);
            }
        }
    }

    private void CreateUserModules()
    {
        // Recherche des modules forcés par la configuration
        foreach (var module in config.Modules)
        {
            var moduleName = $"{ModuleIndice}_{module.Name}";
            foreach (var mainClass in module.Classes.Select(c => _classes.Where(cl => cl.Value.Name == c).FirstOrDefault()))
            {
                if (mainClass.Value != null)
                {
                    mainClass.Value.File!.Module = moduleName;
                    mainClass.Value.File!.Tags = config.Tags.Concat(module.Tags ?? []).ToList();
                }
            }
        }

        foreach (var module in config.Modules)
        {
            foreach (var mainClass in module.Classes.Select(c => _classes.Where(cl => cl.Value.Name == c).FirstOrDefault()))
            {
                if (mainClass.Value != null)
                {
                    AffectModule(mainClass.Value, mainClass.Value.File!.Module);
                }
            }
        }
    }

    private TmdClass FeedProperties(IGrouping<string, DbColumn> group, IEnumerable<ConstraintKey>? primaryKeyConstraints, IEnumerable<ConstraintKey>? foreignConstraints)
    {
        var classe = _classes[group.Key];
        if (group.Any())
        {
            var regularProperties = group.Where(p => !foreignConstraints?.Any(f => f.ColumnName == p.ColumnName) ?? true);
            var trigram = regularProperties.FirstOrDefault()?.ColumnName.Split('_').First();
            if (trigram == null || !regularProperties.All(p => p.ColumnName.StartsWith(trigram)) || regularProperties.Any(p => p.ColumnName == trigram) || regularProperties.Count() <= 1)
            {
                trigram = string.Empty;
            }

            classe.Trigram = trigram;
            classe.Properties = group.Select(c => ColumnToProperty(classe, c, trigram, primaryKeyConstraints?.FirstOrDefault(f => f.ColumnName == c.ColumnName), foreignConstraints?.FirstOrDefault(f => f.ColumnName == c.ColumnName))).ToList();
        }

        return classe;
    }

    private async Task<IEnumerable<DbColumn>> GetColumns()
    {
        // Récupération des colonnes
        var columns = await _connection!.QueryAsync<DbColumn>(GetColumnsQuery());
        return columns.Where(c => !config.Exclude.Select(e => e.ToLower()).Contains(c.TableName.ToLower()));
    }

    private async Task<IEnumerable<ConstraintKey>> GetConstraintKeys(string query)
    {
        // Récupération des contraintes
        var keys = await _connection!.QueryAsync<ConstraintKey>(query);

        return keys.Where(c => !config.Exclude.Contains(c.TableName));
    }

    private Task<IEnumerable<ConstraintKey>> GetForeignKeys()
    {
        return GetConstraintKeys(GetForeignKeysQuery());
    }

    private Task<IEnumerable<ConstraintKey>> GetPrimaryKeys()
    {
        return GetConstraintKeys(GetPrimaryKeysQuery());
    }

    private Task<IEnumerable<ConstraintKey>> GetUniqueKeys()
    {
        return GetConstraintKeys(GetUniqueKeysQuery());
    }

    private void GroupFiles()
    {
        // Regroupement des fichiers qui ont les mêmes imports
        var hasChanged = true;
        while (hasChanged)
        {
            hasChanged = false;
            foreach (var file in Files)
            {
                foreach (var f in Files.Where(f => f.Classes.Count > 0 && Files.ToList().IndexOf(f) > Files.ToList().IndexOf(file)))
                {
                    if (string.Join(string.Empty, f.Uses.Select(u => u.Name)) == string.Join(string.Empty, file.Uses.Select(u => u.Name)) && f.Module == file.Module)
                    {
                        hasChanged = true;
                        file.Classes.AddRange(f.Classes);
                        foreach (var c in f.Classes)
                        {
                            c.File = file;
                        }

                        f.Classes = [];
                        break;
                    }
                }

                if (hasChanged)
                {
                    break;
                }
            }
        }
    }

    private void InitClasses(IEnumerable<IGrouping<string, DbColumn>> classGroups)
    {
        foreach (var group in classGroups)
        {
            var className = group.Key.ToLower().ToPascalCase();
            if (classGroups.Select(c => c.Key).Where(k => k != group.Key && k.ToLower().ToPascalCase() == className).Any())
            {
                className = group.Key;
            }

            if (config.ClassNameOverrides.TryGetValue(className, out var classNameOverride))
            {
                className = classNameOverride;
            }

            _classes.Add(group.Key, new TmdClass() { Name = className, SqlName = group.Key });
        }
    }

    private void InitConnection()
    {
        if (Passwords.TryGetValue(config.Source.DbName, out var password))
        {
            config.Source.Password = password;
        }

        try
        {
            _connection = GetConnection();
            logger.LogInformation($"Connexion à la base de données {config.Source.DbName}...");
            _connection.Open();
        }
        catch (Exception)
        {
            logger.LogInformation($"Mot de passe{(password != null ? " erroné" : string.Empty)} pour l'utilisateur {config.Source.User}:  ");
            Passwords.Remove(config.Source.DbName);
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }

                password += key.KeyChar;
            }

            config.Source.Password = password;
            InitConnection();
        }
    }

    private async Task InitProperties(IEnumerable<IGrouping<string, DbColumn>> classGroups)
    {
        var primaryKeys = await GetPrimaryKeys();
        var foreignKeys = await GetForeignKeys();
        var foreignKeysGroups = foreignKeys.GroupBy(c => c.TableName);
        var primaryKeysGroups = primaryKeys.GroupBy(c => c.TableName);
        foreach (var group in classGroups)
        {
            FeedProperties(group, primaryKeysGroups.FirstOrDefault(f => f.Key == group.Key), foreignKeysGroups.FirstOrDefault(f => f.Key == group.Key));
        }

        // Résolution des contraintes de clés étrangères
        foreach (var group in classGroups)
        {
            ResolveForeignProperties(group, foreignKeysGroups.FirstOrDefault(f => f.Key == group.Key));
        }
    }

    private async Task InitUniqConstraints(IEnumerable<IGrouping<string, DbColumn>> classGroups)
    {
        var uniqueKeys = await GetUniqueKeys();

        // Résolution des contraintes d'unicité
        var uniqueKeysGroups = uniqueKeys.GroupBy(c => c.TableName);
        foreach (var group in classGroups)
        {
            var classe = _classes[group.Key];
            var uniqConstraints = uniqueKeysGroups.Where(f => f.Key == group.Key);
            if (uniqConstraints != null)
            {
                AddUniqConstraints(classe, uniqConstraints);
            }
        }
    }

    private async Task ReadValues()
    {
        // Extraction des valeurs pour les tables paramétrées
        foreach (var classe in config.ExtractValues.Select(classe => _classes.FirstOrDefault(c => classe == c.Value.Name)).Where(c => c.Value != null))
        {
            var values = await _connection!.QueryAsync(@$"select * from {classe.Key}");
            classe.Value.Values.AddRange(values.Select(r =>
            {
                var d = new Dictionary<string, string?>();
                foreach (var kv in r)
                {
                    d.Add(classe.Value.Properties.OfType<TmdRegularProperty>().First(p => p.SqlName == kv.Key).Name, kv.Value?.ToString());
                }

                return d;
            }));
        }
    }

    private void RenameFiles()
    {
        // Renommage des fichiers
        foreach (var group in Files.GroupBy(f => f.Module))
        {
            var indice = 1;
            foreach (var file in group.OrderBy(f => f.Name))
            {
                var mainClass = file.Classes.OrderByDescending(cl => cl.Dependencies.Count + _classes.SelectMany(c => c.Value.Dependencies).Where(c => c == cl).Count()).First();
                file.Name = (indice < 10 ? "0" : string.Empty) + indice++ + "_" + mainClass.Name;
                file.Path = Path.Combine(config.OutputDirectory, file.Module!, file.Name);
            }
        }
    }

    private void ResolveForeignProperties(IGrouping<string, DbColumn> group, IGrouping<string, ConstraintKey>? foreignKeys)
    {
        var classe = _classes[group.Key];
        if (foreignKeys != null)
        {
            foreach (var fk in classe.Properties.OfType<TmdAssociationProperty>())
            {
                var foreignColumnName = foreignKeys.First(p => p.ColumnName == fk.SqlName).ForeignColumnName;
                fk.ForeignProperty = fk.Association!.Properties.OfType<TmdRegularProperty>().First(p => p.SqlName == foreignColumnName);
            }
        }
    }

    private IEnumerable<string> WriteFiles()
    {
        foreach (var file in Files)
        {
            var fileName = Path.Combine(ModelRoot, $"{file.Path}.tmd");
            yield return fileName;

            using var fileWriter = writerProvider.OpenFileWriter(fileName, logger);
            using var tmdFileWriter = new TmdWriter(fileWriter, file, ModelRoot);
        }
    }
}