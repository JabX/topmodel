using Microsoft.Extensions.DependencyInjection;
using TopModel.Core.Loaders;
using TopModel.Utils;

namespace TopModel.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddModelStore(this IServiceCollection services, FileChecker fileChecker, ModelConfig config)
    {
        services
            .AddMemoryCache()
            .AddSingleton(fileChecker)
            .AddSingleton<ClassLoader>()
            .AddSingleton<ConverterLoader>()
            .AddSingleton<DataFlowLoader>()
            .AddSingleton<DecoratorLoader>()
            .AddSingleton<DomainLoader>()
            .AddSingleton<EndpointLoader>()
            .AddSingleton<PropertyLoader>()
            .AddSingleton<ModelFileLoader>()
            .AddSingleton<ModelConfig>()
            .AddSingleton<TranslationStore>()
            .AddSingleton<ModelStore>()
            .AddSingleton(config);

        return services;
    }

    public static void FixConfig(this ModelConfig config, string rootDir)
    {
        config.ModelRoot ??= string.Empty;
        config.LockFileName ??= "topmodel.lock";
        ModelUtils.TrimSlashes(config, c => c.ModelRoot);
        ModelUtils.CombinePath(rootDir, config, c => c.ModelRoot);
        ModelUtils.CombinePath(rootDir, config.I18n, c => c.RootPath);
    }
}