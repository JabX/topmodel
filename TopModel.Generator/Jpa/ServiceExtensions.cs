﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TopModel.Core;
using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Jpa;

public static class ServiceExtensions
{
    public static IServiceCollection AddJpa(this IServiceCollection services, string dn, IEnumerable<JpaConfig>? configs)
    {
        if (configs != null)
        {
            for (var i = 0; i < configs.Count(); i++)
            {
                var config = configs.ElementAt(i);
                var number = i + 1;

                CombinePath(dn, config, c => c.OutputDirectory);
                TrimSlashes(config, c => c.ApiRootPath);
                TrimSlashes(config, c => c.ModelRootPath);
                TrimSlashes(config, c => c.ResourceRootPath);

                if (config.EntitiesPackageName != null || config.DtosPackageName != null)
                {
                    services
                        .AddSingleton<IModelWatcher>(p =>
                            new JpaModelGenerator(p.GetRequiredService<ILogger<JpaModelGenerator>>(), config, p.GetRequiredService<ModelConfig>()) { Number = number });
                    services
                        .AddSingleton<IModelWatcher>(p =>
                            new JpaModelInterfaceGenerator(p.GetRequiredService<ILogger<JpaModelInterfaceGenerator>>(), config) { Number = number });
                }

                if (config.DaosPackageName != null)
                {
                    services
                        .AddSingleton<IModelWatcher>(p =>
                            new JpaDaoGenerator(p.GetRequiredService<ILogger<JpaDaoGenerator>>(), config) { Number = number });
                }

                if (config.ResourceRootPath != null)
                {
                    services
                        .AddSingleton<IModelWatcher>(p =>
                            new JpaResourceGenerator(p.GetRequiredService<ILogger<JpaResourceGenerator>>(), config, p.GetRequiredService<TranslationStore>()) { Number = number });
                }

                if (config.ApiRootPath != null)
                {
                    if (config.ApiGeneration == ApiGeneration.Server)
                    {
                        services
                            .AddSingleton<IModelWatcher>(p =>
                                new SpringServerApiGenerator(p.GetRequiredService<ILogger<SpringServerApiGenerator>>(), config) { Number = number });
                    }
                    else if (config.ApiGeneration == ApiGeneration.Client)
                    {
                        services
                            .AddSingleton<IModelWatcher>(p =>
                                new SpringClientApiGenerator(p.GetRequiredService<ILogger<SpringClientApiGenerator>>(), config) { Number = number });
                    }
                }
            }
        }

        return services;
    }
}
