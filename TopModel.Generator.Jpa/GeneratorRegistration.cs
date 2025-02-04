using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;
using TopModel.Generator.Jpa.ClassGeneration;
using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Jpa;

public class GeneratorRegistration : IGeneratorRegistration<JpaConfig>
{
    /// <inheritdoc cref="IGeneratorRegistration{T}.Register" />
    public void Register(IServiceCollection services, JpaConfig config, int number)
    {
        if (config.EnumsValuesPath == "default")
        {
            config.EnumsValuesPath = config.EnumsPath;
        }

        TrimSlashes(config, c => c.EntitiesPath);
        TrimSlashes(config, c => c.DaosPath);
        TrimSlashes(config, c => c.DtosPath);
        TrimSlashes(config, c => c.EnumsPath);
        TrimSlashes(config, c => c.EnumsValuesPath);
        TrimSlashes(config, c => c.ApiPath);
        TrimSlashes(config, c => c.ResourcesPath);

        config.Language ??= "java";

        services.AddGenerator<JavaDtoGenerator, JpaConfig>(config, number);
        if (config.UseJdbc)
        {
            services.AddGenerator<JdbcEntityGenerator, JpaConfig>(config, number);
        }
        else
        {
            if (config.EnumsAsEnums)
            {
                services.AddGenerator<JpaEnumValuesGenerator, JpaConfig>(config, number);
            }
            else
            {
                services.AddGenerator<JpaEnumGenerator, JpaConfig>(config, number);
                services.AddGenerator<JpaEnumEntityGenerator, JpaConfig>(config, number);
                services.AddGenerator<JavaEnumDtoGenerator, JpaConfig>(config, number);
            }

            services.AddGenerator<JpaEntityGenerator, JpaConfig>(config, number);
        }

        services.AddGenerator<JpaModelInterfaceGenerator, JpaConfig>(config, number);
        services.AddGenerator<JpaMapperGenerator, JpaConfig>(config, number);
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
                switch (config.ClientApiGeneration)
                {
                    case ClientApiMode.RestClient:
                        services.AddGenerator<SpringClientApiGenerator, JpaConfig>(config, number);
                        break;
                    case ClientApiMode.RestTemplate:
                        services.AddGenerator<SpringRestTemplateApiGenerator, JpaConfig>(config, number);
                        break;
                    case ClientApiMode.FeignClient:
                        services.AddGenerator<FeignClientApiGenerator, JpaConfig>(config, number);
                        break;
                }
            }
        }
    }
}
