using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SharpYaml.Serialization;
using TopModel.ModelGenerator;
using TopModel.ModelGenerator.Database;
using TopModel.ModelGenerator.OpenApi;
using TopModel.Utils;

var command = new RootCommand("Lance le générateur de fichiers tmd.") { Name = "tmdgen" };
var watchMode = false;
var checkMode = false;
var regularCommand = false;
var configs = new List<(string FullPath, string DirectoryName)>();
var serializer = new Serializer(new() { NamingConvention = new CamelCaseNamingConvention() });

var fileOption = new Option<IEnumerable<FileInfo>>(["-f", "--file"], "Chemin vers un fichier de config.");
var watchOption = new Option<bool>(["-w", "--watch"], "Lance le générateur en mode 'watch'");
var checkOption = new Option<bool>(["-c", "--check"], "Vérifie que le modèle généré est conforme aux sources.");
command.AddOption(fileOption);
command.AddOption(watchOption);
command.AddOption(checkOption);
command.SetHandler(
    (files, watch, check) =>
    {
        regularCommand = true;
        watchMode = watch;
        checkMode = check;

        void HandleFile(FileInfo file)
        {
            configs.Add((file.FullName, file.DirectoryName!));
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
            var pattern = "tmdgen*.config";
            foreach (var fileName in Directory.GetFiles(dir, pattern, SearchOption.AllDirectories))
            {
                var foundFile = new FileInfo(fileName);
                if (foundFile != null)
                {
                    HandleFile(foundFile);
                }
            }

            if (!configs.Any())
            {
                var found = false;
                while (!found && dir != null)
                {
                    dir = Directory.GetParent(dir)?.FullName;
                    if (dir != null)
                    {
                        foreach (var fileName in Directory.GetFiles(dir, pattern))
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
    watchOption,
    checkOption);

await command.InvokeAsync(args);

if (!regularCommand)
{
    return 0;
}

if (!configs.Any())
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Aucun fichier de config trouvé.");
    return 1;
}

var fullVersion = Assembly.GetEntryAssembly()!.GetName().Version!;
var version = $"{fullVersion.Major}.{fullVersion.Minor}.{fullVersion.Build}";
var colors = new[] { ConsoleColor.DarkCyan, ConsoleColor.DarkYellow, ConsoleColor.Cyan, ConsoleColor.Yellow };

Console.WriteLine($"======= TopModel.ModelGenerator v{version} =======");
Console.WriteLine();

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

for (var p = 0; p < configs.Count; p++)
{
    var (fullName, _) = configs[p];
    Console.ForegroundColor = colors[p % colors.Length];
    Console.Write($"#{p + 1} - ");
    Console.WriteLine(Path.GetRelativePath(Directory.GetCurrentDirectory(), fullName));
}

var disposables = new List<IDisposable>();
var loggerProvider = new LoggerProvider();

var fsCache = new MemoryCache(new MemoryCacheOptions());
Dictionary<string, string> passwords = [];

async Task StartGeneration(string filePath, string directoryName, int i)
{
    Console.WriteLine();

    var configFile = new FileInfo(filePath);
    using var stream = configFile.OpenRead();
    var config = serializer.Deserialize<ModelGeneratorConfig>(stream)!;

    config.ConfigRoot = directoryName;
    config.ModelRoot ??= "./";
    config.LockFileName ??= "tmdgen.lock";
    ModelUtils.CombinePath(directoryName, config, c => c.ModelRoot);

    var services = new ServiceCollection()
        .AddLogging(builder => builder.AddProvider(loggerProvider))
        .AddSingleton<IFileWriterProvider>(new GeneratedFileWriterProvider(config));

    foreach (var conf in config.OpenApi)
    {
        ModelUtils.TrimSlashes(conf, c => c.OutputDirectory);
        services.AddSingleton<ModelGenerator>(p => new OpenApiTmdGenerator(p.GetRequiredService<ILogger<OpenApiTmdGenerator>>(), conf, p.GetRequiredService<IFileWriterProvider>())
        {
            DirectoryName = directoryName,
            ModelRoot = config.ModelRoot,
            Number = config.OpenApi.IndexOf(conf) + 1
        });
    }

    foreach (var conf in config.Database)
    {
        ModelUtils.TrimSlashes(conf, c => c.OutputDirectory);
        if (conf.Source.DbType == DbType.ORACLE)
        {
            services.AddSingleton<ModelGenerator>(p => new DatabaseOraTmdGenerator(p.GetRequiredService<ILogger<DatabaseOraTmdGenerator>>(), conf, p.GetRequiredService<IFileWriterProvider>())
            {
                DirectoryName = directoryName,
                ModelRoot = config.ModelRoot,
                Number = config.Database.IndexOf(conf) + 1,
                Passwords = passwords
            });
        }
        else if (conf.Source.DbType == DbType.POSTGRESQL)
        {
            services.AddSingleton<ModelGenerator>(p => new DatabasePgTmdGenerator(p.GetRequiredService<ILogger<DatabasePgTmdGenerator>>(), conf, p.GetRequiredService<IFileWriterProvider>())
            {
                DirectoryName = directoryName,
                ModelRoot = config.ModelRoot,
                Number = config.Database.IndexOf(conf) + 1,
                Passwords = passwords
            });
        }
        else if (conf.Source.DbType == DbType.MYSQL)
        {
            services.AddSingleton<ModelGenerator>(p => new DatabaseMySqlTmdGenerator(p.GetRequiredService<ILogger<DatabaseMySqlTmdGenerator>>(), conf, p.GetRequiredService<IFileWriterProvider>())
            {
                DirectoryName = directoryName,
                ModelRoot = config.ModelRoot,
                Number = config.Database.IndexOf(conf) + 1,
                Passwords = passwords
            });
        }
    }

    using var provider = services.BuildServiceProvider();

    var mainLogger = provider.GetRequiredService<ILogger<ModelGenerator>>();
    var loggingScope = new LoggingScope(i + 1, colors[i]);
    using var scope = mainLogger.BeginScope(loggingScope);

    var generators = provider.GetRequiredService<IEnumerable<ModelGenerator>>();

    mainLogger.LogInformation($"Générateurs enregistrés :\n                          {string.Join("\n                          ", generators.Select(g => $"- {g.Name}@{{{g.Number}}}"))}");

    var tmdLock = new TopModelLock(config, mainLogger);
    var generatedFiles = new List<string>();

    foreach (var generator in generators)
    {
        generatedFiles.AddRange(await generator.Generate(loggingScope));
    }

    tmdLock.UpdateFiles(generatedFiles);

    mainLogger.LogInformation("Mise à jour terminée avec succès.");
}

foreach (var config in configs)
{
    await StartGeneration(config.FullPath, config.DirectoryName, configs.IndexOf(config));

    if (watchMode)
    {
        var fsWatcher = new FileSystemWatcher(config.DirectoryName, "tmdgen*.config");
        fsWatcher.Changed += (sender, args) =>
        {
            fsCache.Set(args.FullPath, args, new MemoryCacheEntryOptions()
                .AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(TimeSpan.FromMilliseconds(500)).Token))
                .RegisterPostEvictionCallback(async (k, v, r, a) =>
                {
                    if (r != EvictionReason.TokenExpired)
                    {
                        return;
                    }

                    await StartGeneration(args.FullPath, config.DirectoryName, configs.IndexOf(config));
                }));
        };
        fsWatcher.IncludeSubdirectories = true;
        fsWatcher.EnableRaisingEvents = true;
        disposables.Add(fsWatcher);
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

    foreach (var disposable in disposables)
    {
        disposable.Dispose();
    }
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

return 0;