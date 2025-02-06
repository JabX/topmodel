using TopModel.ModelGenerator.Database;
using TopModel.ModelGenerator.OpenApi;
using TopModel.Utils;

namespace TopModel.ModelGenerator;

internal class ModelGeneratorConfig : ConfigBase
{
    public List<OpenApiConfig> OpenApi { get; set; } = [];

    public List<DatabaseConfig> Database { get; set; } = [];
}
