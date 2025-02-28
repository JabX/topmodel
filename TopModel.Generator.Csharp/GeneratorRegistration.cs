using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;

using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Csharp;

public class GeneratorRegistration : IGeneratorRegistration<CsharpConfig>
{
    /// <inheritdoc cref="IGeneratorRegistration{T}.Register" />
    public void Register(IServiceCollection services, CsharpConfig config, int number)
    {
        TrimSlashes(config, c => c.ApiFilePath);
        TrimSlashes(config, c => c.ApiRootPath);
        TrimSlashes(config, c => c.DbContextPath);
        TrimSlashes(config, c => c.ReferenceAccessorsImplementationPath);
        TrimSlashes(config, c => c.ReferenceAccessorsInterfacePath);
        TrimSlashes(config, c => c.NonPersistentModelPath);
        TrimSlashes(config, c => c.PersistentModelPath);
        TrimSlashes(config, c => c.ReferencesModelPath);

        config.Language ??= "csharp";

        config.ReferenceAccessorsImplementationPath ??= Path.Combine(config.DbContextPath ?? string.Empty, "Reference");

        services.AddGenerator<CSharpClassGenerator, CsharpConfig>(config, number);
        services.AddGenerator<MapperGenerator, CsharpConfig>(config, number);

        if (config.DbContextPath != null)
        {
            services.AddGenerator<DbContextGenerator, CsharpConfig>(config, number);
        }

        if (config.Kinetix && config.ReferenceAccessorsInterfacePath != null)
        {
            services.AddGenerator<ReferenceAccessorGenerator, CsharpConfig>(config, number);
        }

        if (config.ApiGeneration != null)
        {
            if (config.ApiGeneration != ApiGeneration.Client)
            {
                services.AddGenerator<CSharpApiServerGenerator, CsharpConfig>(config, number);
            }

            if (config.ApiGeneration != ApiGeneration.Server)
            {
                services.AddGenerator<CSharpApiClientGenerator, CsharpConfig>(config, number);
            }
        }

        if (config.DataFlowsPath != null)
        {
            services.AddGenerator<DataFlowGenerator, CsharpConfig>(config, number);
        }
    }
}
