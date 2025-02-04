using System.Text;
using Microsoft.Extensions.Logging;

namespace TopModel.Utils;

/// <summary>
/// Provider pour récupérer des File Writers.
/// </summary>
public class GeneratedFileWriterProvider(ConfigBase config)
{
    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="fileName">Nom du fichier à écrire.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="encoderShouldEmitUTF8Identifier">UTF8 avec BOM ?</param>
    /// <returns>GeneratedFileWriter</returns>
    public GeneratedFileWriter OpenFileWriter(string fileName, ILogger logger, bool encoderShouldEmitUTF8Identifier = true)
    {
        return new GeneratedFileWriter(config, fileName, logger, encoderShouldEmitUTF8Identifier);
    }

    /// <summary>
    /// Crée une nouvelle instance.
    /// </summary>
    /// <param name="fileName">Nom du fichier à écrire.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="encoding">Encoding</param>
    /// <returns>GeneratedFileWriter</returns>
    public GeneratedFileWriter OpenFileWriter(string fileName, ILogger logger, Encoding encoding)
    {
        return new GeneratedFileWriter(config, fileName, logger, encoding);
    }
}
