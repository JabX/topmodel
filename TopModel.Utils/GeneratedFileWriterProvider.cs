using System.Text;
using Microsoft.Extensions.Logging;

namespace TopModel.Utils;

/// <summary>
/// Implémentation par défaut de IFileWriterProvider.
/// </summary>
public class GeneratedFileWriterProvider(ConfigBase config) : IFileWriterProvider
{
    /// <inheritdoc cref="IFileWriterProvider.OpenFileWriter(string, ILogger, bool)" />
    public IFileWriter OpenFileWriter(string fileName, ILogger logger, bool encoderShouldEmitUTF8Identifier = true)
    {
        return new GeneratedFileWriter(config, fileName, logger, encoderShouldEmitUTF8Identifier);
    }

    /// <inheritdoc cref="IFileWriterProvider.OpenFileWriter(string, ILogger, Encoding)" />
    public IFileWriter OpenFileWriter(string fileName, ILogger logger, Encoding encoding)
    {
        return new GeneratedFileWriter(config, fileName, logger, encoding);
    }
}
