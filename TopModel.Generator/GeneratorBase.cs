﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;

namespace TopModel.Generator;

public abstract class GeneratorBase<T> : IModelWatcher
{
    private readonly GeneratorConfigBase<T> _config;
    private readonly ILogger _logger;

    protected GeneratorBase(ILogger logger, GeneratorConfigBase<T> config)
    {
        _config = config;
        _logger = logger;
    }

    public abstract string Name { get; }

    public int Number { get; init; }

    public virtual IEnumerable<string> GeneratedFiles => new List<string>();

    protected Dictionary<string, ModelFile> Files { get; } = new();

    protected IEnumerable<Class> Classes => Files.SelectMany(f => f.Value.Classes).Distinct();

    public void OnErrors(IDictionary<ModelFile, IEnumerable<ModelError>> errors)
    {
    }

    public void OnFilesChanged(IEnumerable<ModelFile> files, ModelStoreConfig? storeConfig = null)
    {
        using var scope = _logger.BeginScope(((IModelWatcher)this).FullName);
        using var scope2 = _logger.BeginScope(storeConfig);

        var handledFiles = files.Where(file => _config.Tags.Intersect(file.Tags).Any());

        foreach (var file in handledFiles)
        {
            Files[file.Name] = file;
        }

        HandleFiles(handledFiles);
    }

    protected abstract void HandleFiles(IEnumerable<ModelFile> files);
}