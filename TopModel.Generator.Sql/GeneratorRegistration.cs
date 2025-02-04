using Microsoft.Extensions.DependencyInjection;
using TopModel.Generator.Core;
using TopModel.Generator.Sql.Procedural;
using TopModel.Generator.Sql.Ssdt;

using static TopModel.Utils.ModelUtils;

namespace TopModel.Generator.Sql;

public class GeneratorRegistration : IGeneratorRegistration<SqlConfig>
{
    /// <inheritdoc cref="IGeneratorRegistration{T}.Register" />
    public void Register(IServiceCollection services, SqlConfig config, int number)
    {
        config.Language ??= "sql";

        if (config.Ssdt != null)
        {
            CombinePath(config.OutputDirectory, config.Ssdt, c => c.InitListScriptFolder);
            CombinePath(config.OutputDirectory, config.Ssdt, c => c.TableScriptFolder);
            CombinePath(config.OutputDirectory, config.Ssdt, c => c.TableTypeScriptFolder);

            if (config.Ssdt.TableScriptFolder != null)
            {
                services.AddGenerator<SsdtTableGenerator, SqlConfig>(config, number);
            }

            if (config.Ssdt.TableTypeScriptFolder != null)
            {
                services.AddGenerator<SsdtTableTypeGenerator, SqlConfig>(config, number);
            }

            if (config.Ssdt.InitListScriptFolder != null)
            {
                services.AddGenerator<SsdtReferenceListGenerator, SqlConfig>(config, number);

                if (config.Ssdt.InitListMainScriptName != null)
                {
                    services.AddGenerator<SsdtMainReferenceListGenerator, SqlConfig>(config, number);
                }
            }
        }

        if (config.Procedural != null)
        {
            CombinePath(config.OutputDirectory, config.Procedural, c => c.CrebasFile);
            CombinePath(config.OutputDirectory, config.Procedural, c => c.IndexFKFile);
            CombinePath(config.OutputDirectory, config.Procedural, c => c.InitListFile);
            CombinePath(config.OutputDirectory, config.Procedural, c => c.TypeFile);
            CombinePath(config.OutputDirectory, config.Procedural, c => c.UniqueKeysFile);
            CombinePath(config.OutputDirectory, config.Procedural, c => c.CommentFile);
            CombinePath(config.OutputDirectory, config.Procedural, c => c.ResourceFile);

            services.AddGenerator<ProceduralSqlGenerator, SqlConfig>(config, number);
        }
    }
}
