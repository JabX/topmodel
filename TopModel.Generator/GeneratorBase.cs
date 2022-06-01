﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;

namespace TopModel.Generator;

public abstract class GeneratorBase : IModelWatcher
{
    private readonly GeneratorConfigBase _config;
    private readonly ILogger _logger;

    protected GeneratorBase(ILogger logger, GeneratorConfigBase config)
    {
        _config = config;
        _logger = logger;
    }

    public abstract string Name { get; }

    public int Number { get; set; }

    public void OnErrors(IDictionary<ModelFile, IEnumerable<ModelError>> errors)
    {
    }

    public void OnFilesChanged(IEnumerable<ModelFile> files)
    {
        using var scope = _logger.BeginScope(((IModelWatcher)this).FullName);
        HandleFiles(files.Where(file => _config.Tags.Intersect(file.Tags).Any()));
    }

    public virtual List<string> GetGeneratedFiles(ModelStore modelStore)
    {
        return new List<string>();
    }

    protected abstract void HandleFiles(IEnumerable<ModelFile> files);
}