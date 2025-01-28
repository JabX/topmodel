using System.Text;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur des objets de traduction javascripts.
/// </summary>
public class JpaResourceGenerator : TranslationGeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaResourceGenerator> _logger;
    private readonly TranslationStore _translationStore;

    public JpaResourceGenerator(ILogger<JpaResourceGenerator> logger, TranslationStore translationStore)
        : base(logger, translationStore)
    {
        _logger = logger;
        _translationStore = translationStore;
    }

    public override string Name => "JpaResourceGen";

    protected override string? GetResourceFilePath(IProperty property, string tag, string lang)
    {
        var p = property.ResourceProperty;
        if (p.Label != null || (p.Class?.Values.Any() ?? false) && p.Class?.DefaultProperty != null)
        {
            return Path.Combine(
                Config.OutputDirectory,
                Config.ResolveVariables(Config.ResourcesPath!, tag: tag, lang: lang).ToLower(),
                $"{p.Parent.Namespace.RootModule.ToKebabCase()}{(string.IsNullOrEmpty(lang) ? string.Empty : $"_{lang}")}.properties");
        }

        return null;
    }

    protected override void HandleResourceFile(string filePath, string lang, IEnumerable<IProperty> properties)
    {
        var encoding = Encoding.Latin1;
        if (Config.ResourcesEncoding is not null)
        {
            encoding = Config.ResourcesEncoding switch
            {
                ResourcesEncoding.UTF8 => Encoding.UTF8,
                _ => Encoding.Latin1
            };
        }

        using var fw = new FileWriter(filePath, _logger, encoding) { EnableHeader = false };
        var containers = properties.GroupBy(prop => prop.Parent);

        foreach (var container in containers.OrderBy(c => c.Key.NameCamel))
        {
            WriteClasse(fw, container, lang);
        }
    }

    /// <summary>
    /// Générère le noeus de classe.
    /// </summary>
    /// <param name="fw">Flux de sortie.</param>
    /// <param name="container">Classe.</param>
    /// <param name="lang">Langue de traduction.</param>
    protected void WriteClasse(FileWriter fw, IGrouping<IPropertyContainer, IProperty> container, string lang)
    {
        if (Config.TranslateProperties == true)
        {
            foreach (var property in container)
            {
                if (property.Label != null)
                {
                    fw.WriteLine($"{property.ResourceKey}={_translationStore.GetTranslation(property, lang)}");
                }
            }
        }

        if (container.Key is Class classe && classe.DefaultProperty != null && Config.TranslateReferences == true)
        {
            foreach (var val in classe.Values)
            {
                fw.WriteLine($"{val.ResourceKey}={_translationStore.GetTranslation(val, lang)}");
            }
        }
    }
}