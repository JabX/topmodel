using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;

using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Jpa;

public class GeneratorRegistration : IGeneratorRegistration<JpaConfig>
{
    /// <inheritdoc cref="IGeneratorRegistration{T}.Register" />
    public void Register(IServiceCollection services, JpaConfig config, int number)
    {
        TrimSlashes(config, c => c.EntitiesPath);
        TrimSlashes(config, c => c.DaosPath);
        TrimSlashes(config, c => c.DtosPath);
        TrimSlashes(config, c => c.EnumsPath);
        TrimSlashes(config, c => c.ApiPath);
        TrimSlashes(config, c => c.EnumConverterPath);
        TrimSlashes(config, c => c.ResourcesPath);

        config.Language ??= "java";

        services.AddGenerator<JpaModelGenerator, JpaConfig>(config, number);
        services.AddGenerator<JpaModelInterfaceGenerator, JpaConfig>(config, number);
        services.AddGenerator<JpaMapperGenerator, JpaConfig>(config, number);
        services.AddGenerator<JpaEnumGenerator, JpaConfig>(config, number);
        if (config.EnumsAsEnum)
        {
            services.AddGenerator<JpaConverterGenerator, JpaConfig>(config, number);
        }

        if (config.DaosPath != null)
        {
            services.AddGenerator<JpaDaoGenerator, JpaConfig>(config, number);
        }

        if (config.DataFlowsPath != null)
        {
            services.AddGenerator<SpringDataFlowGenerator, JpaConfig>(config, number);
        }

        if (config.ResourcesPath != null)
        {
            services.AddGenerator<JpaResourceGenerator, JpaConfig>(config, number);
        }

        if (config.ApiGeneration != null)
        {
            if (config.ApiGeneration != ApiGeneration.Client)
            {
                services.AddGenerator<SpringServerApiGenerator, JpaConfig>(config, number);
            }

            if (config.ApiGeneration != ApiGeneration.Server)
            {
                if (config.ClientApiGeneration == ClientApiMode.RestClient)
                {
                    services.AddGenerator<SpringClientApiGenerator, JpaConfig>(config, number);
                }
                else
                {
                    services.AddGenerator<SpringRestTemplateApiGenerator, JpaConfig>(config, number);
                }
            }
        }
    }
}
