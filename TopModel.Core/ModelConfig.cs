﻿#nullable disable
namespace TopModel.Core;

public class ModelConfig
{
    public string App { get; set; }

    public string ModelRoot { get; set; }

    public IList<ModelErrorType> NoWarn { get; set; } = new List<ModelErrorType>();

    public bool AllowCompositePrimaryKey { get; set; }

    public bool PluralizeTableNames { get; set; }

    public string GetFileName(string filePath)
    {
        return Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), ModelRoot), filePath)
            .Replace(".tmd", string.Empty)
            .Replace("\\", "/");
    }
}