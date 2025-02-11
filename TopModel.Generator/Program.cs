using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Packaging.Core;
using TopModel.Core;
using TopModel.Core.Loaders;
using TopModel.Generator;
using TopModel.Generator.Core;
using TopModel.Utils;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

var fileChecker = new FileChecker("schema.config.json");

var configs = new Dictionary<string, ModelConfig>();
var excludedTags = Array.Empty<string>();
var watchMode = false;
var checkMode = false;
string? updateMode = null;
var schemaMode = false;
var regularCommand = false;
var returnCode = 0;

var command = new RootCommand("Lance le générateur topmodel.") { Name = "modgen" };

var fileOption = new Option<IEnumerable<FileInfo>>(["-f", "--file"], "Chemin vers un fichier de config.");
var excludeOption = new Option<IEnumerable<string>>(["-e", "--exclude"], "Tag à ignorer lors de la génération.");
var watchOption = new Option<bool>(["-w", "--watch"], "Lance le générateur en mode 'watch'");
var checkOption = new Option<bool>(["-c", "--check"], "Vérifie que le code généré est conforme au modèle.");
var updateOption = new Option<string>(["-u", "--update"], "Met à jour le module de générateurs spécifié (ou tous les modules si 'all').");
var schemaOption = new Option<bool>(["-s", "--schema"], "Génère le fichier de schéma JSON du fichier de config.");
command.AddOption(fileOption);
command.AddOption(excludeOption);
command.AddOption(watchOption);
command.AddOption(checkOption);
command.AddOption(updateOption);
command.AddOption(schemaOption);

// Crée la sous-commande 'init'
var initCommand = new Command("init", "Initialise le générateur topmodel.")
{
    Handler = new InitCommandHandler()
};

command.AddCommand(initCommand);

command.SetHandler(
    (files, excludes, watch, update, check, schema) =>
    {
        regularCommand = true;
        excludedTags = excludes.ToArray();
        watchMode = watch;
        checkMode = check;
        updateMode = update;
        schemaMode = schema;

        void HandleFile(FileInfo file)
        {
            try
            {
                fileChecker.CheckConfigFile(file.FullName);
                using var textToRead = file.OpenText();
                var text = textToRead.ReadToEnd();
                var config = fileChecker.DeserializeConfig(text).Init(file.DirectoryName!);
                configs.Add(file.FullName, config);
            }
            catch (ModelException me)
            {
                returnCode = 1;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(me.Message);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        if (files.Any())
        {
            foreach (var file in files)
            {
                if (!file.Exists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Le fichier '{file.FullName}' est introuvable.");
                }
                else
                {
                    HandleFile(file);
                }
            }
        }
        else
        {
            var dir = Directory.GetCurrentDirectory();
            var pattern = new Regex("topmodel\\.?([a-zA-Z-_.]*)\\.config$");

            void SearchConfigFile(string dirName, int depth = 0)
            {
                if (depth > 3)
                {
                    return;
                }

                foreach (var entryName in Directory.EnumerateFileSystemEntries(dirName))
                {
                    if (Directory.Exists(entryName))
                    {
                        SearchConfigFile(entryName, depth + 1);
                    }
                    else if (pattern.IsMatch(entryName))
                    {
                        HandleFile(new FileInfo(entryName));
                    }
                }
            }

            SearchConfigFile(dir);

            if (configs.Count == 0)
            {
                var found = false;
                while (!found && dir != null)
                {
                    dir = Directory.GetParent(dir)?.FullName;
                    if (dir != null)
                    {
                        foreach (var fileName in Directory.EnumerateFiles(dir).Where(f => pattern.IsMatch(f)))
                        {
                            HandleFile(new FileInfo(fileName));
                            found = true;
                        }
                    }
                }
            }
        }
    },
    fileOption,
    excludeOption,
    watchOption,
    updateOption,
    checkOption,
    schemaOption);

await command.InvokeAsync(args);

if (!regularCommand)
{
    return returnCode;
}

if (configs.Count == 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Aucun fichier de configuration trouvé.");
    Console.ForegroundColor = ConsoleColor.Gray;
    return 1;
}

var fullVersion = Assembly.GetEntryAssembly()!.GetName().Version!;
var version = $"{fullVersion.Major}.{fullVersion.Minor}.{fullVersion.Build}";

var colors = new[] { ConsoleColor.DarkCyan, ConsoleColor.DarkYellow, ConsoleColor.Cyan, ConsoleColor.Yellow };

Console.WriteLine($"========= TopModel.Generator v{version} =========");
Console.WriteLine();

var latestVersion = await NugetUtils.GetLatestVersionAsync("TopModel.Generator");
if (latestVersion != null && latestVersion.Version != version)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Nouvelle version disponible : {latestVersion.Version}");
    Console.WriteLine("Vous pouvez lancer la commande `dotnet tool update -g TopModel.Generator` pour effectuer la mise à jour.");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine();
}

if (excludedTags.Length > 0)
{
    Console.Write("Tags");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write(" exclus ");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($"de la génération : {string.Join(", ", excludedTags)}.");
}

if (updateMode != null)
{
    Console.Write("Mode");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write(" update ");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($"activé pour : {updateMode}.");

    await NugetUtils.ClearAsync();
}

if (watchMode)
{
    Console.Write("Mode");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write(" watch ");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("activé.");
}

if (checkMode)
{
    Console.Write("Mode");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.Write(" check ");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("activé.");
}

Console.WriteLine("Fichiers de configuration trouvés :");

for (var i = 0; i < configs.Count; i++)
{
    var fullName = configs.ElementAt(i).Key;
    Console.ForegroundColor = colors[i % colors.Length];
    Console.Write($"#{i + 1} - ");
    Console.WriteLine(Path.GetRelativePath(Directory.GetCurrentDirectory(), fullName));
}

static Type? GetIGenRegInterface(Type t)
{
    return t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGeneratorRegistration<>));
}

static (Type Type, string Name) GetIGenRegInterfaceAndName(Type generator)
{
    var configType = GetIGenRegInterface(generator)!.GetGenericArguments()[0];
    var configName = configType.Name.Replace("Config", string.Empty).ToLower();
    return (configType, configName);
}

var dotnetMajor = Environment.Version.Major;
var disposables = new List<IDisposable>();
var loggerProvider = new LoggerProvider();
var hasErrors = Enumerable.Range(0, configs.Count).Select(_ => false).ToArray();
var modgenAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.ManifestModule.Name).ToHashSet();

for (var i = 0; i < configs.Count; i++)
{
    var (fullName, config) = configs.ElementAt(i);

    var storeConfig = new LoggingScope(i + 1, colors[i % colors.Length]);
    var logger = loggerProvider.CreateLogger("TopModel.Generator");
    using var scope = logger.BeginScope(storeConfig);
    var topModelLock = new TopModelLock(config, logger);

    Console.WriteLine();

    var modgenRoot = Path.GetFullPath(".modgen", config.ModelRoot);

    if (updateMode == "all")
    {
        topModelLock.Modules = [];

        if (Directory.Exists(modgenRoot))
        {
            Directory.Delete(modgenRoot, true);
        }
    }
    else if (updateMode != null)
    {
        topModelLock.Modules.Remove(updateMode);

        foreach (var module in Directory.GetFileSystemEntries(modgenRoot).Where(p => p.Split('/').Last().Contains(updateMode)))
        {
            Directory.Delete(module, true);
        }
    }

    foreach (var cg in config.CustomGenerators)
    {
        Directory.CreateDirectory(modgenRoot);

        var customDir = Path.GetFullPath(Path.Combine(new FileInfo(Path.GetFullPath(fullName)).DirectoryName!, cg));

        var customHash = GetHash(
            Directory
                .EnumerateFiles(
                    Path.GetFullPath(cg, new FileInfo(fullName).DirectoryName!),
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")),
            customDir) ?? string.Empty;

        var customHashLocalFile = Path.Combine(modgenRoot, cg.Replace("/", "-").Replace("\\", "-"));
        var customHashLocal = File.Exists(customHashLocalFile) ? await File.ReadAllTextAsync(customHashLocalFile) : string.Empty;

        if (!topModelLock.Custom.TryGetValue(cg, out var customLockHash) || customHash != customLockHash || customHash != customHashLocal)
        {
            logger.LogInformation($"Build de '{cg}' en cours...");
            var build = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "dotnet",
                Arguments = "build -v q",
                WorkingDirectory = customDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            });

            await build!.StandardOutput.ReadToEndAsync();
            await build!.StandardError.ReadToEndAsync();
            await build!.WaitForExitAsync();

            if (build.ExitCode != 0)
            {
                logger.LogError($"Erreur lors du build de '{cg}'");
                logger.LogError((await build.StandardOutput.ReadToEndAsync()).Trim());
                return 1;
            }

            logger.LogInformation($"Build de '{cg}' terminé.");
            topModelLock.Custom ??= [];
            topModelLock.Custom[cg] = customHash;
            await File.WriteAllTextAsync(customHashLocalFile, topModelLock.Custom[cg]);
        }
    }

    var generators = new List<Type>();
    var deps = new List<ModgenDependency>();

    if (Environment.GetEnvironmentVariable("LOCAL_DEV") != null)
    {
        var generatorsPath = Path.Combine(new FileInfo(Assembly.GetEntryAssembly()!.Location).DirectoryName!, "../../../..");
        var modules = Directory.GetFileSystemEntries(generatorsPath).Where(e => e.Contains("TopModel.Generator.") && !e.Contains("TopModel.Generator.Core"));
        config.CustomGenerators.AddRange(modules.Select(m => Path.GetRelativePath(new FileInfo(fullName).DirectoryName!, m).Replace("\\", "/")));
    }

    foreach (var cg in config.CustomGenerators)
    {
        string? csproj = null;
        if (Directory.Exists(Path.GetFullPath(cg, new FileInfo(fullName).DirectoryName!)))
        {
            csproj = Directory.GetFiles(Path.GetFullPath(cg, new FileInfo(fullName).DirectoryName!), "*.csproj").FirstOrDefault();
        }

        if (csproj == null)
        {
            logger.LogError($"Aucun fichier csproj trouvé pour le module de générateurs '{cg}'.");
            returnCode = 1;
            continue;
        }

        var csprojXml = new XmlDocument();
        csprojXml.LoadXml(File.ReadAllText(csproj));

        foreach (var dep in csprojXml.GetElementsByTagName("PackageReference").Cast<XmlNode>()
            .Where(n => n.ChildNodes.Count == 0)
            .ToDictionary(n => n.Attributes!["Include"]!.Value, n => n.Attributes!["Version"]!.Value)
            .Where(n => n.Key.StartsWith("TopModel.Generator")))
        {
            if (dep.Key == "TopModel.Generator.Core")
            {
                var depVersion = dep.Value.Split('.').Select(int.Parse).ToArray();
                if (depVersion[0] != fullVersion.Major)
                {
                    logger.LogError($"Le module de générateurs '{cg}' ne référence pas la bonne version majeure de TopModel ({dep.Value} < {version}).");
                    returnCode = 1;
                    continue;
                }
                else if (depVersion[1] > fullVersion.Minor)
                {
                    logger.LogError($"Le module de générateurs '{cg}' référence une version plus récente de TopModel ({dep.Value} > {version}).");
                    returnCode = 1;
                    continue;
                }
            }
            else
            {
                var configKey = dep.Key.Split('.').Last().ToLower();
                if (!topModelLock.Modules.TryGetValue(configKey, out var ev))
                {
                    topModelLock.Modules.Add(configKey, new() { Version = dep.Value });
                }
                else if (ev.Version != dep.Value)
                {
                    logger.LogError($"Le module personalisé '{cg}' référence le module '{configKey}' en version '{dep.Value}', ce qui n'est pas la version du lockfile ('{ev}').");
                    returnCode = 1;
                    continue;
                }
            }
        }

        if (returnCode == 0)
        {
            var assemblies = new DirectoryInfo(Path.Combine(Path.GetFullPath(cg, new FileInfo(fullName).DirectoryName!), "bin"))
                .GetFiles($"*.dll", SearchOption.AllDirectories)
                .Where(a => !modgenAssemblies.Contains(a.Name))
                .DistinctBy(a => a.Name)
                .Select(f => Assembly.LoadFrom(f.FullName))
                .ToList();

            generators.AddRange(assemblies
                .Where(a => a.ManifestModule.Name.Equals($"{cg.Split('/').Last().ToLower()}.dll", StringComparison.CurrentCultureIgnoreCase))
                .SelectMany(a => a.GetExportedTypes())
                .Where(t => GetIGenRegInterface(t) != null));
        }
    }

    if (returnCode != 0)
    {
        continue;
    }

    var resolvedConfigKeys = new Dictionary<string, string>();

    foreach (var configKey in config.Generators.Keys)
    {
        if (generators.Any(g => GetIGenRegInterfaceAndName(g).Name == configKey))
        {
            resolvedConfigKeys.Add(configKey, "custom");
            continue;
        }

        var fullModuleName = $"TopModel.Generator.{configKey.ToFirstUpper()}";

        if (!topModelLock.Modules.TryGetValue(configKey, out var moduleVersion))
        {
            moduleVersion = await NugetUtils.GetLatestVersionAsync(fullModuleName, forceCheck: true);

            if (moduleVersion == null)
            {
                logger.LogError($"Aucun module de générateurs trouvé pour '{configKey}'.");
                returnCode = 1;
                continue;
            }

            topModelLock.Modules.Add(configKey, moduleVersion);
        }

        deps.Add(new(configKey, moduleVersion));
    }

    var hasInstalled = false;

    if (deps.Count > 0)
    {
        Directory.CreateDirectory(modgenRoot);

        foreach (var dep in deps)
        {
            var depVersion = dep.Version.Version;
            var moduleFolder = Path.Combine(modgenRoot, $"{dep.ConfigKey}.{depVersion}");

            var depHash = GetFolderHash(moduleFolder);

            if (depHash == null || depHash != dep.Version.Hash)
            {
                if (Directory.Exists(moduleFolder))
                {
                    logger.LogInformation($"({dep.ConfigKey}) Module corrompu, réinstallation...");
                    Directory.Delete(moduleFolder, true);
                }

                logger.LogInformation($"({dep.ConfigKey}) Installation de {dep.FullName}@{depVersion} en cours...");

                if (!await NugetUtils.DoesPackageExistsAsync(dep.FullName, depVersion))
                {
                    logger.LogError($"({dep.ConfigKey}) Le package {dep.FullName}@{depVersion} est introuvable.");
                    returnCode = 1;
                    continue;
                }

                Directory.CreateDirectory(moduleFolder);

                using var packageReader = await NugetUtils.DownloadPackageAsync(dep.FullName, depVersion);
                var nuspecReader = await packageReader.GetNuspecReaderAsync(CancellationToken.None);

                var dependencyGroup = nuspecReader.GetDependencyGroups()
                    .OrderByDescending(dg => dg.TargetFramework.Version.Major)
                    .First(dg => dg.TargetFramework.Version.Major <= dotnetMajor);

                var dependencies = dependencyGroup.Packages;
                var framework = dependencyGroup.TargetFramework.ToString();

                File.WriteAllText(Path.Combine(moduleFolder, "min-version"), dependencies.Single(d => d.Id == "TopModel.Generator.Core").VersionRange.MinVersion!.ToString());

                foreach (var file in packageReader.GetFiles().Where(f => f == $"lib/{framework}/{dep.FullName}.dll" || f.EndsWith("config.json")))
                {
                    packageReader.ExtractFile(file, Path.Combine(moduleFolder, file.Split('/').Last()), NullLogger.Instance);
                }

                var installedDependencies = new List<string>();
                dependencies = dependencies.Where(d => d.Id != "TopModel.Generator.Core");

                while (dependencies.Any())
                {
                    var newDeps = new List<PackageDependency>();
                    foreach (var otherDep in dependencies)
                    {
                        using var packageReaderDep = await NugetUtils.DownloadPackageAsync(otherDep.Id, otherDep.VersionRange.MinVersion!.ToString());
                        var file = packageReaderDep.GetFiles().SingleOrDefault(f => f.StartsWith($"lib/{framework}") && f.EndsWith(".dll") && !f.EndsWith(".resources.dll"));
                        if (file != null)
                        {
                            packageReaderDep.ExtractFile(file, Path.Combine(moduleFolder, file.Split('/').Last()), NullLogger.Instance);

                            installedDependencies.Add(otherDep.Id);

                            var nuspecReaderDep = await packageReaderDep.GetNuspecReaderAsync(CancellationToken.None);
                            if (nuspecReaderDep.GetDependencyGroups().Any())
                            {
                                newDeps.AddRange(nuspecReaderDep.GetDependencyGroups()
                                    .Single(dg => dg.TargetFramework.ToString() == framework)
                                    .Packages
                                    .Where(dep => !installedDependencies.Contains(dep.Id)));
                            }
                        }
                    }

                    dependencies = newDeps;
                }

                hasInstalled = true;
                logger.LogInformation($"({dep.ConfigKey}) Installation de {dep.FullName}@{depVersion} terminée avec succès.");
                dep.Version.Hash = GetFolderHash(moduleFolder);
            }

            var minVersionText = File.ReadAllText(Path.Combine(moduleFolder, "min-version"));
            var minVersion = minVersionText.Split('.').Select(int.Parse).ToArray();
            if (minVersion[0] != fullVersion.Major)
            {
                logger.LogError($"Le module '{dep.ConfigKey}' ne référence pas la bonne version majeure de TopModel ({depVersion} < {version}).");
                returnCode = 1;
                continue;
            }
            else if (minVersion[1] > fullVersion.Minor)
            {
                logger.LogError($"Le module '{dep.ConfigKey}' référence une version plus récente de TopModel ({minVersionText} > {version}).");
                returnCode = 1;
                continue;
            }

            generators.AddRange(Directory.GetFiles(moduleFolder, "*.dll").SelectMany(a => Assembly.LoadFrom(a).GetExportedTypes().Where(t => GetIGenRegInterface(t) != null)));
            resolvedConfigKeys.Add(dep.ConfigKey, depVersion);
        }
    }

    if (returnCode != 0)
    {
        continue;
    }

    topModelLock.Write();

    foreach (var dep in deps)
    {
        dep.LatestVersion = (await NugetUtils.GetLatestVersionAsync(dep.FullName))?.Version;
    }

    logger.LogInformation($"Générateurs utilisés :{Environment.NewLine}                          {string.Join($"{Environment.NewLine}                          ", resolvedConfigKeys.Select(rck => $"- {rck.Key}: {rck.Value}"))}");

    var depsToUpdate = deps.Where(dep => dep.LatestVersion != null && dep.LatestVersion != dep.Version.Version);
    if (depsToUpdate.Any())
    {
        logger.LogWarning($"Il existe une mise à jour pour les générateurs suivants :{Environment.NewLine}                          {string.Join($"{Environment.NewLine}                          ", depsToUpdate.Select(dep => $"- {dep.ConfigKey}: {dep.Version.Version} -> {dep.LatestVersion}"))}");
        logger.LogWarning($"Vous pouvez lancer la commande `modgen --update {(depsToUpdate.Count() == 1 ? depsToUpdate.Single().ConfigKey : "all")}` pour effectuer la mise à jour.");
    }

    if (schemaMode || hasInstalled)
    {
        logger.LogInformation("Génération du schéma de configuration...");

        var schema = JsonNode.Parse(File.ReadAllText(FileChecker.GetFilePath(Assembly.GetExecutingAssembly(), "schema.config.json")))!.AsObject();

        schema.Remove("additionalProperties");
        schema.Add("additionalProperties", false);

        foreach (var generator in generators)
        {
            var (configType, configName) = GetIGenRegInterfaceAndName(generator);

            var configSchema = JsonNode.Parse(@"{""type"": ""array""}")!.AsObject();
            configSchema.Add("items", JsonNode.Parse(File.ReadAllText(FileChecker.GetFilePath(configType.Assembly, $"{configName}.config.json"))));
            schema["properties"]!.AsObject().Add(configName, configSchema);
        }

        File.WriteAllText(fullName + ".schema.json", schema.Root.ToJsonString(new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true }));
        var configFile = File.ReadAllText(fullName);
        if (!configFile.StartsWith("# yaml-language-server"))
        {
            var relativePath = fullName.ToRelative(config.ConfigRoot);
            configFile = $"# yaml-language-server: $schema={relativePath}.schema.json \n" + configFile;
            File.WriteAllText(fullName, configFile);
        }

        logger.LogInformation("Schéma de configuration généré avec succès.");
    }

    var services = new ServiceCollection()
        .AddTransient(typeof(ILogger<>), typeof(Logger<>))
        .AddTransient<ILoggerFactory, LoggerFactory>()
        .AddSingleton<ILoggerProvider>(loggerProvider)
        .AddSingleton<IFileWriterProvider>(new GeneratedFileWriterProvider(config))
        .AddModelStore(fileChecker, config);

    var hasError = false;

    foreach (var generator in generators)
    {
        var (configType, configName) = GetIGenRegInterfaceAndName(generator);

        if (config.Generators.TryGetValue(configName, out var genConfigMaps))
        {
            for (var j = 0; j < genConfigMaps.Count(); j++)
            {
                var genConfigMap = genConfigMaps.ElementAt(j);
                var number = j + 1;

                try
                {
                    var genConfig = (GeneratorConfigBase)fileChecker.GetGenConfig(configName, configType, genConfigMap);

                    genConfig.ExcludedTags = excludedTags;
                    genConfig.InitVariables(config.App, number);

                    genConfig.TranslateReferences ??= config.I18n.TranslateReferences;
                    genConfig.TranslateProperties ??= config.I18n.TranslateProperties;

                    ModelUtils.TrimSlashes(genConfig, c => c.OutputDirectory);
                    ModelUtils.CombinePath(config.ConfigRoot, genConfig, c => c.OutputDirectory);

                    var instance = Activator.CreateInstance(generator);
                    instance!.GetType().GetMethod("Register")!
                        .Invoke(instance, [services, genConfig, number]);
                }
                catch (ModelException me)
                {
                    hasError = true;
                    returnCode = 1;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(me.Message);
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }
    }

    if (!hasError)
    {
        var provider = services.BuildServiceProvider();
        disposables.Add(provider);

        var modelStore = provider.GetRequiredService<ModelStore>();

        modelStore.DisableLockfile = excludedTags.Length > 0;

        var k = i;
        modelStore.OnResolve += hasError =>
        {
            hasErrors[k] = hasError;
        };

        var watcher = modelStore.LoadFromConfig(watchMode, topModelLock, storeConfig);
        if (watcher != null)
        {
            disposables.Add(watcher);
        }
    }
}

if (watchMode)
{
    var autoResetEvent = new AutoResetEvent(false);
    Console.CancelKeyPress += (sender, eventArgs) =>
    {
        eventArgs.Cancel = true;
        autoResetEvent.Set();
    };
    autoResetEvent.WaitOne();
}

foreach (var provider in disposables)
{
    provider.Dispose();
}

if (hasErrors.Any(he => he))
{
    return 1;
}

if (checkMode && loggerProvider.Changes > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine();
    if (loggerProvider.Changes == 1)
    {
        Console.WriteLine($"1 fichier généré a été modifié ou supprimé. Le code généré n'était pas à jour.");
    }
    else
    {
        Console.WriteLine($"{loggerProvider.Changes} fichiers générés ont été modifiés ou supprimés. Le code généré n'était pas à jour.");
    }

    Console.ForegroundColor = ConsoleColor.Gray;

    return 1;
}

return returnCode;

static string? GetFolderHash(string path)
{
    if (!Directory.Exists(path))
    {
        return null;
    }

    return GetHash(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories), path);
}

static string? GetHash(IEnumerable<string> f, string path)
{
    var md5 = MD5.Create();
    var files = f.OrderBy(f => f).ToList();
    foreach (var file in files)
    {
        var relativePath = Path.GetRelativePath(path, file).Replace("\\", "/");
        var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
        md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

        var contentBytes = File.ReadAllBytes(file);
        if (files.IndexOf(file) == files.Count - 1)
        {
            md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
        }
        else
        {
            md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
        }
    }

    return md5.Hash != null
        ? BitConverter.ToString(md5.Hash).Replace("-", string.Empty).ToLower()
        : null;
}