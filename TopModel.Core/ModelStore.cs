using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using TopModel.Core.FileModel;
using TopModel.Core.Loaders;
using TopModel.Core.Resolvers;
using TopModel.Utils;

namespace TopModel.Core;

public class ModelStore
{
    private readonly ModelConfig _config;
    private readonly IMemoryCache _fsCache;
    private readonly ILogger<ModelStore> _logger;
    private readonly ModelFileLoader _modelFileLoader;

    private readonly Dictionary<string, ModelFile> _modelFiles = [];
    private readonly IEnumerable<IModelWatcher> _modelWatchers;
    private readonly HashSet<string> _pendingUpdates = [];

    private readonly object _puLock = new();

    private readonly TranslationStore _translationStore;

    private LoggingScope? _storeConfig;
    private TopModelLock? _topModelLock;

    public ModelStore(IMemoryCache fsCache, ModelFileLoader modelFileLoader, ILogger<ModelStore> logger, ModelConfig config, IEnumerable<IModelWatcher> modelWatchers, TranslationStore translationStore)
    {
        _config = config;
        _fsCache = fsCache;
        _logger = logger;
        _modelFileLoader = modelFileLoader;
        _translationStore = translationStore;
        _modelWatchers = modelWatchers.Where(mw => !mw.Disabled);
    }

    public event Action<bool>? OnResolve;

    public bool DisableLockfile { get; set; }

    public IEnumerable<Class> Classes => _modelFiles.SelectMany(mf => mf.Value.Classes).Distinct();

    public IEnumerable<Endpoint> Endpoints => _modelFiles.SelectMany(mf => mf.Value.Endpoints).Distinct();

    public IEnumerable<DataFlow> DataFlows => _modelFiles.SelectMany(mf => mf.Value.DataFlows).Distinct();

    public IDictionary<string, Domain> Domains => _modelFiles.SelectMany(mf => mf.Value.Domains)
        .DistinctBy(d => (string)d.Name)
        .ToDictionary(d => (string)d.Name, d => d);

    public IList<Converter> Converters => _modelFiles.SelectMany(mf => mf.Value.Converters).ToList();

    public IEnumerable<Decorator> Decorators => _modelFiles.SelectMany(mf => mf.Value.Decorators).Distinct();

    public IEnumerable<ModelFile> Files => _modelFiles.Values;

    public IEnumerable<Class> GetAvailableClasses(ModelFile file)
    {
        return GetDependencies(file).SelectMany(m => m.Classes)
            .Concat(file.Classes);
    }

    public IEnumerable<DataFlow> GetAvailableDataFlows(ModelFile file)
    {
        return GetDependencies(file).SelectMany(m => m.DataFlows).Concat(file.DataFlows);
    }

    public IEnumerable<Decorator> GetAvailableDecorators(ModelFile file)
    {
        return GetDependencies(file).SelectMany(m => m.Decorators).Concat(file.Decorators);
    }

    public Dictionary<string, Class> GetReferencedClasses(ModelFile modelFile)
    {
        var dependencies = GetDependencies(modelFile).ToList();
        return dependencies
            .SelectMany(m => m.Classes)
            .Concat(modelFile.Classes)
            .Distinct()
            .GroupBy(c => c.Name.Value)
            .ToDictionary(c => c.Key, c => c.First());
    }

    public IDisposable? LoadFromConfig(bool watch = false, TopModelLock? topModelLock = null, LoggingScope? storeConfig = null)
    {
        _storeConfig = storeConfig;
        _topModelLock = topModelLock;

        using var scope = _logger.BeginScope(_storeConfig!);

        var watchers = _modelWatchers.Select(mw => mw.FullName.Split("@")).GroupBy(split => split[0]).Select(grp => $"{grp.Key}@{{{string.Join(",", grp.Select(split => split[1]))}}}");
        if (watchers.Count() > 0)
        {
            _logger.LogInformation($"Watchers enregistrés : \n                          - {string.Join("\n                          - ", watchers.OrderBy(x => x))}");
        }
        else
        {
            _logger.LogWarning($"Aucun watchers enregistré pour cette configuration");
        }

        FileSystemWatcher? fsWatcher = null;
        if (watch)
        {
            fsWatcher = new FileSystemWatcher(_config.ModelRoot, "*.tmd");
            fsWatcher.Changed += OnFSChangedEvent;
            fsWatcher.Created += OnFSChangedEvent;
            fsWatcher.Deleted += OnFSChangedEvent;
            fsWatcher.Renamed += OnFSChangedEvent;
            fsWatcher.IncludeSubdirectories = true;
            fsWatcher.EnableRaisingEvents = true;
        }

        _modelFiles.Clear();
        _pendingUpdates.Clear();

        _logger.LogInformation("Chargement du modèle...");

        var files = Directory.EnumerateFiles(_config.ModelRoot, "*.tmd", SearchOption.AllDirectories);

        lock (_puLock)
        {
            foreach (var file in files)
            {
                LoadFile(file);
            }
        }

        LoadTranslations();
        TryApplyUpdates();

        return fsWatcher;
    }

    public void OnModelFileChange(string filePath, string? content = null)
    {
        _logger.LogInformation(string.Empty);
        _logger.LogInformation($"Modifié:  {filePath.ToRelative()}");

        lock (_puLock)
        {
            LoadFile(filePath, content);
        }

        TryApplyUpdates();
    }

    public void TryApplyUpdates()
    {
        if (_pendingUpdates.Count == 0)
        {
            return;
        }

        lock (_puLock)
        {
            try
            {
                var referenceErrors = new List<ModelError>();

                var affectedFiles = _pendingUpdates.Select(pu => _modelFiles.TryGetValue(pu, out var mf) ? mf : null).Any(mf => mf?.Domains.Count > 0 || mf?.Converters.Count > 0)
                    ? _modelFiles.Values
                    : GetAffectedFiles(_pendingUpdates).Distinct();

                IList<ModelFile> sortedFiles = new List<ModelFile>(1);
                try
                {
                    sortedFiles = CoreUtils.Sort(affectedFiles, f => GetDependencies(f).Where(d => affectedFiles.Any(af => af.Name == d.Name)));
                }

                // Dépendance circulaire.
                catch (ModelException e) when (e.ModelError is not null)
                {
                    referenceErrors.Add(e.ModelError);
                }

                foreach (var affectedFile in sortedFiles)
                {
                    referenceErrors.AddRange(ResolveReferences(affectedFile));
                }

                referenceErrors.AddRange(GetGlobalErrors());

                Parallel.ForEach(_modelWatchers, modelWatcher =>
                {
                    modelWatcher.OnErrors(affectedFiles
                        .Select(file => (file, errors: referenceErrors.Where(e => e.File == file && !_config.NoWarn.Contains(e.ModelErrorType))))
                        .ToDictionary(i => i.file, i => i.errors));
                });

                foreach (var error in referenceErrors.Where(e => e.IsError))
                {
                    _logger.LogError(error.ToString());
                }

                foreach (var error in referenceErrors.Where(e => !e.IsError && !_config.NoWarn.Contains(e.ModelErrorType)))
                {
                    _logger.LogWarning(error.ToString());
                }

                var hasError = referenceErrors.Any(r => r.IsError);
                OnResolve?.Invoke(hasError);

                if (hasError)
                {
                    throw new ModelException("Erreur lors de la lecture du modèle.");
                }
                else
                {
                    _logger.LogInformation("Modèle chargé avec succès.");
                }

                Parallel.ForEach(_modelWatchers, modelWatcher =>
                {
                    modelWatcher.OnFilesChanged(sortedFiles, _storeConfig);
                });

                var generatedFiles = _modelWatchers.Where(m => m.GeneratedFiles != null).SelectMany(m => m.GeneratedFiles!);
                if (generatedFiles.Any() && !DisableLockfile && _topModelLock != null)
                {
                    _topModelLock.UpdateFiles(generatedFiles);
                }

                _logger.LogInformation($"Mise à jour terminée avec succès.");

                _pendingUpdates.Clear();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }

    private IEnumerable<ModelFile> GetAffectedFiles(IEnumerable<string> fileNames, HashSet<string>? foundFiles = null)
    {
        foundFiles ??= [];

        foreach (var file in _modelFiles.Values.Where(f => fileNames.Contains(f.Name) || f.Uses.Any(d => fileNames.Contains(d.ReferenceName))))
        {
            if (!foundFiles.Contains(file.Name))
            {
                foundFiles.Add(file.Name);
                yield return file;

                foreach (var use in GetAffectedFiles([file.Name], foundFiles))
                {
                    yield return use;
                }
            }
        }
    }

    private IEnumerable<ModelFile> GetDependencies(ModelFile modelFile)
    {
        return modelFile.Uses
            .Select(dep => _modelFiles.TryGetValue(dep.ReferenceName, out var depFile) ? depFile : null!)
            .Where(dep => dep != null)
            .Concat(Files.Where(f => f != modelFile && f.Converters.Count > 0 && modelFile.Classes.Any(c => c.FromMappers.Count > 0 || c.ToMappers.Count > 0)))
            .Concat(Files.Where(f => f != modelFile && f.Domains.Count > 0 && (modelFile.Domains.Count == 0 || modelFile.Domains.Any(d => d.AsDomainReferences.Count > 0))));
    }

    private IEnumerable<ModelError> GetGlobalErrors()
    {
        foreach (var g in Files.SelectMany(f => f.Domains).GroupBy(f => (string)f.Name).Where(g => g.Count() > 1))
        {
            foreach (var domain in g.Skip(1))
            {
                yield return new ModelError(domain, $"Le domaine '{domain}' est déjà défini") { ModelErrorType = ModelErrorType.TMD0007 };
            }
        }

        foreach (var converter in Converters)
        {
            var dup = Converters.FirstOrDefault(c => Converters.IndexOf(c) < Converters.IndexOf(converter) && c.Conversions.Intersect(converter.Conversions).Any());
            if (dup != null)
            {
                foreach (var (from, to) in dup.Conversions.Intersect(converter.Conversions))
                {
                    yield return new ModelError(converter, $"La définition de la conversion entre {from.Name} et {to.Name} est déjà définie dans un autre converter") { ModelErrorType = ModelErrorType.TMD1022 };
                }
            }
        }

        foreach (var classe in Classes.Where(c => c.Trigram != null && Classes.Any(u => u.Trigram == c.Trigram && u != c)))
        {
            yield return new ModelError(classe.ModelFile, $"Le trigram '{classe.Trigram}' est déjà utilisé dans la (les) classe(s) suivantes : {string.Join(", ", Classes.Where(u => u.Trigram == classe.Trigram && u != classe).Select(c => c.Name))}", classe.Trigram.GetLocation()) { IsError = false, ModelErrorType = ModelErrorType.TMD9002 };
        }

        foreach (var domain in Domains.Values.Where(domain => !this.GetDomainReferences(domain).Any()))
        {
            yield return new ModelError(domain, $"Le domaine '{domain.Name}' n'est pas utilisé.") { IsError = false, ModelErrorType = ModelErrorType.TMD9004 };
        }

        foreach (var decorator in Decorators.Where(decorator => !this.GetDecoratorReferences(decorator).Any()))
        {
            yield return new ModelError(decorator, $"Le décorateur '{decorator.Name}' n'est pas utilisé.") { IsError = false, ModelErrorType = ModelErrorType.TMD9005 };
        }

        foreach (var files in Files.GroupBy(file => new { file.Options.Endpoints.FileName, file.Namespace.Module }))
        {
            var endpoints = files.SelectMany(f => f.Endpoints);

            foreach (var endpoint in endpoints.Where((e, i) => files.SelectMany(f => f.Endpoints).Where((p, j) => p.Name == e.Name && j < i).Any()))
            {
                yield return new ModelError(endpoint, $"Le nom '{endpoint.Name}' est déjà utilisé.", endpoint.Name.GetLocation()) { IsError = true, ModelErrorType = ModelErrorType.TMD0003 };
            }

            if (files.Select(file => file.Options.Endpoints.Prefix).Distinct().Count() > 1)
            {
                foreach (var file in files)
                {
                    if (file.Options.Endpoints.Prefix != null)
                    {
                        yield return new ModelError(file, $"Le préfixe d'endpoint '{file.Options.Endpoints.Prefix}' doit être identique à celui de tous les fichiers de même nom et de même module.", file.Options.Endpoints.Prefix?.GetLocation()) { ModelErrorType = ModelErrorType.TMD1021 };
                    }
                    else
                    {
                        yield return new ModelError(file, $"Le fichier ne définit pas de préfixe d'endpoint alors que d'autres fichiers de même nom et de même module le font.") { ModelErrorType = ModelErrorType.TMD1021 };
                    }
                }
            }
        }
    }

    private void LoadFile(string filePath, string? content = null)
    {
        try
        {
            ModelFile? file = null;
            if (File.Exists(filePath))
            {
                file = _modelFileLoader.LoadModelFile(filePath, content);
            }

            if (file != null)
            {
                _modelFiles[file.Name] = file;
                _pendingUpdates.Add(file.Name);
            }
            else
            {
                var fileName = _config.GetFileName(filePath);
                _modelFiles.Remove(fileName);
                _pendingUpdates.Add(fileName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    private void LoadTranslations()
    {
        _translationStore.Translations[_config.I18n.DefaultLang] = [];

        foreach (var lang in _config.I18n.Langs)
        {
            var langMap = new Dictionary<string, string>();
            var directoryPath = _config.I18n.RootPath.Replace("{lang}", lang);
            var exists = Directory.Exists(directoryPath);
            if (exists)
            {
                var files = Directory.GetFiles(directoryPath, "*.properties", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var lines = File.ReadAllLines(file);
                    foreach (var line in lines)
                    {
                        if (line != null && line != string.Empty)
                        {
                            langMap[line.Split("=")[0]] = line.Split("=")[1];
                        }
                    }
                }
            }

            _translationStore.Translations[lang] = langMap;
        }
    }

    private void OnFSChangedEvent(object sender, FileSystemEventArgs e)
    {
        _fsCache.Set(e.FullPath, e, new MemoryCacheEntryOptions()
            .AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token))
            .RegisterPostEvictionCallback((k, v, r, a) =>
            {
                if (r != EvictionReason.TokenExpired)
                {
                    return;
                }

                OnModelFileChange((string)k);
            }));
    }

    private IEnumerable<ModelError> ResolveReferences(ModelFile modelFile)
    {
        var nonExistingFiles = modelFile.Uses.Where(use => !_modelFiles.TryGetValue(use.ReferenceName, out var _));
        foreach (var use in nonExistingFiles)
        {
            yield return new ModelError(modelFile, $"Le fichier référencé '{use.ReferenceName}' est introuvable.", use) { ModelErrorType = ModelErrorType.TMD1007 };
        }

        var duplicatedUses = modelFile.Uses
            .GroupBy(u => u.ReferenceName)
            .Select(u => new { ReferenceName = u.Key, Count = u.Count() })
            .Where(r => r.Count > 1)
            .Select(u => u.ReferenceName);

        foreach (var use in modelFile.Uses.Where(u => duplicatedUses.Contains(u.ReferenceName)).Skip(1))
        {
            yield return new ModelError(modelFile, $"L'import '{use.ReferenceName}' ne doit être spécifié qu'une seule fois", use) { IsError = true, ModelErrorType = ModelErrorType.TMD0002 };
        }

        var dependencies = GetDependencies(modelFile).ToList();

        var referencedClassesRaw = dependencies
            .SelectMany(m => m.Classes)
            .Concat(modelFile.Classes)
            .Distinct();

        var duplicateClasses = referencedClassesRaw
            .GroupBy(c => c.Name.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.OrderByDescending(c => (c.ModelFile == modelFile ? 1_000_000 : 0) + c.Name.Location.Start.Line).First());

        foreach (var classe in duplicateClasses.Where(c => c.ModelFile == modelFile))
        {
            yield return new ModelError(classe, $"La classe '{classe}' est définie plusieurs fois dans le fichier ou une de ses dépendences.", classe.Name.Location) { ModelErrorType = ModelErrorType.TMD0005 };
        }

        var referencedClasses = referencedClassesRaw
            .Where(c => !duplicateClasses.Select(c => c.Name.Value).Contains(c.Name.Value))
            .ToDictionary(c => c.Name.Value, c => c);

        var referencedDecorators = dependencies
            .SelectMany(m => m.Decorators)
            .Concat(modelFile.Decorators)
            .Distinct()
            .ToDictionary(d => (string)d.Name, c => c);

        var referencedDataFlowsRaw = dependencies
             .SelectMany(m => m.DataFlows)
             .Concat(modelFile.DataFlows)
             .Distinct();

        var duplicateDataFlows = referencedDataFlowsRaw
            .GroupBy(c => c.Name.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.OrderByDescending(c => (c.ModelFile == modelFile ? 1_000_000 : 0) + c.Name.Location.Start.Line).First());

        foreach (var dataFlow in duplicateDataFlows.Where(c => c.ModelFile == modelFile))
        {
            yield return new ModelError(dataFlow, $"Le flux de données '{dataFlow}' est défini plusieurs fois dans le fichier ou une de ses dépendences.", dataFlow.Name.Location) { ModelErrorType = ModelErrorType.TMD0008 };
        }

        var referencedDataFlows = referencedDataFlowsRaw
            .Where(c => !duplicateDataFlows.Select(c => c.Name.Value).Contains(c.Name.Value))
            .ToDictionary(c => c.Name.Value, c => c);

        var classResolver = new ClassResolver(modelFile, referencedClasses);
        var dataFlowResolver = new DataFlowResolver(modelFile, referencedDataFlows, referencedClasses);
        var decoratorResolver = new DecoratorResolver(modelFile, referencedDecorators);
        var domainResolver = new DomainResolver(modelFile, Domains, Converters);
        var endpointResolver = new EndpointResolver(modelFile);
        var mapperResolver = new MapperResolver(modelFile, referencedClasses, Converters, _config.UseLegacyAssociationCompositionMappers);
        var propertyResolver = new PropertyResolver(modelFile, Domains, referencedClasses);

        foreach (var error in domainResolver.ResolveAsDomains())
        {
            yield return error;
        }

        foreach (var error in classResolver.ResolveExtends())
        {
            yield return error;
        }

        foreach (var error in decoratorResolver.ResolveDecorators())
        {
            yield return error;
        }

        foreach (var error in propertyResolver.ResolveNonAliasProperties())
        {
            yield return error;
        }

        propertyResolver.ResetAliases();

        // Résolution des alias des décorateurs
        foreach (var error in propertyResolver.ResolveAliases(alp => alp.Decorator is not null && alp.Reference is not null))
        {
            yield return error;
        }

        decoratorResolver.CopyDecoratorProperties();

        // Résolution des alias des classes et endpoints.
        foreach (var error in propertyResolver.ResolveAliases(alp => alp.Decorator is null && alp.Reference is not null))
        {
            yield return error;
        }

        foreach (var error in propertyResolver.ResolveAssociationProperties())
        {
            yield return error;
        }

        foreach (var error in classResolver.ResolveUniqueKeys())
        {
            yield return error;
        }

        foreach (var error in classResolver.ResolveValues())
        {
            yield return error;
        }

        foreach (var error in classResolver.ResolveSpecialProperties())
        {
            yield return error;
        }

        foreach (var error in classResolver.ResolveEnums())
        {
            yield return error;
        }

        foreach (var error in domainResolver.ResolveConverters())
        {
            yield return error;
        }

        foreach (var error in mapperResolver.ResolveMappers())
        {
            yield return error;
        }

        foreach (var error in dataFlowResolver.ResolveDataFlows())
        {
            yield return error;
        }

        foreach (var error in classResolver.CheckResult())
        {
            yield return error;
        }

        foreach (var error in endpointResolver.CheckResult())
        {
            yield return error;
        }

        classResolver.ResolveTranslations(_translationStore, _config.I18n.DefaultLang);

        foreach (var use in modelFile.UselessImports.Where(u => dependencies.Any(d => d.Name == u.ReferenceName)))
        {
            yield return new ModelError(modelFile, $"L'import '{use.ReferenceName}' n'est pas utilisé.", use) { IsError = false, ModelErrorType = ModelErrorType.TMD9001 };
        }
    }
}