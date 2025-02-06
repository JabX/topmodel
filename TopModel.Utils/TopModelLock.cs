using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TopModel.Utils;

public class TopModelLock : TopModelLockFile
{
    private readonly ConfigBase _config;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly ILogger _logger;
    private readonly ISerializer _serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitEmptyCollections)
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithIndentedSequences()
        .Build();

    [SetsRequiredMembers]
    public TopModelLock(ConfigBase config, ILogger logger)
    {
        _config = config;
        _logger = logger;

        var lockFile = new FileInfo(Path.Combine(_config.ConfigRoot, _config.LockFileName));

        if (lockFile.Exists)
        {
            try
            {
                using var file = lockFile.OpenText();
                var lf = _deserializer.Deserialize<TopModelLockFile>(file);
                Version = lf.Version;
                GeneratedFiles = lf.GeneratedFiles;
                Modules = lf.Modules ?? [];
                Custom = lf.Custom ?? [];
            }
            catch
            {
                _logger.LogError($"Erreur à la lecture du fichier {_config.LockFileName}. Merci de rétablir la version générée automatiquement.");
                throw;
            }
        }

        var assembly = Assembly.GetEntryAssembly()!.GetName()!;
        var version = $"{assembly.Version!.Major}.{assembly.Version!.Minor}.{assembly.Version!.Build}";

        if (Version != null && version != Version)
        {
            logger.LogWarning($"Ce modèle a été généré pour la dernière fois avec {assembly.Name} v{Version}, qui n'est pas la version actuellement installée (v{version})");
        }

        Version = version;
    }

    public void UpdateFiles(IEnumerable<string> generatedFiles)
    {
        GeneratedFiles ??= [];

        generatedFiles = generatedFiles.Select(g => g.Replace("\\", "/"));

        var generatedFilesList = generatedFiles
            .Select(f => f.ToRelative(_config.ConfigRoot))
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var filesToPrune = GeneratedFiles
            .Select(f => f.Replace("\\", "/"))
            .Where(f => !generatedFilesList.Select(gf => isWindows ? gf.ToLowerInvariant() : gf).Contains(isWindows ? f.ToLowerInvariant() : f))
            .Select(f => Path.Combine(_config.ConfigRoot, f));

        Parallel.ForEach(filesToPrune.Where(File.Exists), fileToPrune =>
        {
            File.Delete(fileToPrune);
            _logger.LogInformation($"Supprimé: {fileToPrune.ToRelative()}");
        });

        GeneratedFiles = generatedFilesList;

        if (!_config.NoWarn.Contains(ModelErrorType.TMD8001))
        {
            foreach (var ignoredFile in _config.IgnoredFiles.Select(i => Path.Combine(_config.ConfigRoot, i.Path).Replace("\\", "/")).Except(generatedFiles))
            {
                _logger.LogWarning($"{{TMD8001}} - Le fichier '{ignoredFile.ToRelative(_config.ConfigRoot)}' dans `ignoredFiles` est introuvable.");
            }
        }

        Write();
    }

    public void Write()
    {
        if (Modules.Count > 0 || GeneratedFiles.Count > 0)
        {
            using var fw = new GeneratedFileWriter(_config, Path.Combine(_config.ConfigRoot, _config.LockFileName), _logger, true)
            {
                StartCommentToken = "#"
            };

            fw.Write(_serializer.Serialize(this));
        }
    }
}