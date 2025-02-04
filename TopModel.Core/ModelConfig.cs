#nullable disable

using TopModel.Utils;

namespace TopModel.Core;

public class ModelConfig : ConfigBase
{
    public string App { get; set; }

    public IList<ModelErrorType> NoWarn { get; set; } = new List<ModelErrorType>();

    public bool PluralizeTableNames { get; set; }

    public bool UseLegacyRoleNames { get; set; }

    public bool UseLegacyAssociationCompositionMappers { get; set; }

    public I18nConfig I18n { get; set; } = new();

    public Dictionary<string, IEnumerable<IDictionary<string, object>>> Generators { get; } = new();

    public List<string> CustomGenerators { get; set; } = new();

    public string GetFileName(string filePath)
    {
        return Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), ModelRoot), filePath)
            .Replace(".tmd", string.Empty)
            .Replace("\\", "/");
    }
}