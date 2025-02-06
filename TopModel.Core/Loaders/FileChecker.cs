using System.Reflection;
using System.Text;
using NJsonSchema;
using NJsonSchema.Validation;
using TopModel.Utils;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TopModel.Core.Loaders;

public class FileChecker
{
    private readonly JsonSchema? _configSchema;
    private readonly IDeserializer _deserializer;
    private readonly JsonSchema _modelSchema;
    private readonly ISerializer _serializer;

    public FileChecker(string? configSchemaPath = null)
    {
        if (configSchemaPath != null)
        {
            _configSchema = JsonSchema.FromFileAsync(GetFilePath(Assembly.GetExecutingAssembly(), configSchemaPath)).Result;
        }

        _modelSchema = JsonSchema.FromFileAsync(GetFilePath(Assembly.GetExecutingAssembly(), "schema.json")).Result;

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithNodeTypeResolver(new InferTypeFromValueResolver())
            .IgnoreUnmatchedProperties()
            .Build();
        _serializer = new SerializerBuilder()
            .JsonCompatible()
            .Build();
    }

    public static string GetFilePath(Assembly assembly, string fileName)
    {
        return Path.Combine(Path.GetDirectoryName(assembly.Location)!, fileName);
    }

    public void CheckConfigFile(string fileName)
    {
        if (_configSchema != null)
        {
            CheckCore(_configSchema, fileName);
        }
    }

    public void CheckModelFile(string fileName, string? content = null)
    {
        CheckCore(_modelSchema, fileName, content);
    }

    public T Deserialize<T>(string yaml)
    {
        return _deserializer.Deserialize<T>(yaml);
    }

    public T Deserialize<T>(TextReader yaml)
    {
        return _deserializer.Deserialize<T>(yaml);
    }

    public T Deserialize<T>(IParser parser)
    {
        return _deserializer.Deserialize<T>(parser);
    }

    public ModelConfig DeserializeConfig(string yaml)
    {
        var parser = new Parser(new StringReader(yaml));
        var config = new ModelConfig();
        parser.Consume<StreamStart>();
        parser.Consume<DocumentStart>();
        parser.ConsumeMapping(prop =>
        {
            parser.TryConsume<Scalar>(out var value);
            switch (prop.Value)
            {
                case "app":
                    config.App = value!.Value;
                    break;
                case "modelRoot":
                    config.ModelRoot = value!.Value;
                    break;
                case "lockFileName":
                    config.LockFileName = value!.Value;
                    break;
                case "noWarn":
                    parser.ConsumeSequence(() =>
                    {
                        config.NoWarn.Add(Enum.Parse<ModelErrorType>(parser.Consume<Scalar>().Value));
                    });
                    break;
                case "pluralizeTableNames":
                    config.PluralizeTableNames = value!.Value == "true";
                    break;
                case "useLegacyRoleNames":
                    config.UseLegacyRoleNames = value!.Value == "true";
                    break;
                case "useLegacyAssociationCompositionMappers":
                    config.UseLegacyAssociationCompositionMappers = value!.Value == "true";
                    break;
                case "i18n":
                    config.I18n = _deserializer.Deserialize<I18nConfig>(parser);
                    break;
                case "generators":
                    parser.ConsumeSequence(() =>
                    {
                        config.CustomGenerators.Add(parser.Consume<Scalar>().Value);
                    });
                    break;
                case "ignoredFiles":
                    config.IgnoredFiles = _deserializer.Deserialize<IList<IgnoredFile>>(parser);
                    break;
                default:
                    config.Generators.Add(prop.Value, _deserializer.Deserialize<IEnumerable<IDictionary<string, object>>>(parser));
                    break;
            }
        });

        parser.Consume<DocumentEnd>();
        parser.Consume<StreamEnd>();

        return config;
    }

    public object GetGenConfig(string configName, Type configType, IDictionary<string, object> genConfigMap)
    {
        var schema = JsonSchema.FromFileAsync(GetFilePath(configType.Assembly, $"{configName}.config.json")).Result;
        Validate(configName, schema, _serializer.Serialize(genConfigMap));
        return _deserializer.Deserialize(_serializer.Serialize(genConfigMap), configType)!;
    }

    private static void Validate(string fileName, JsonSchema schema, string json)
    {
        var errors = schema.Validate(json);

        if (errors.Any())
        {
            var erreur = new StringBuilder();
            erreur.Append($"Erreur dans le fichier {fileName.ToRelative()} :");

            void HandleErrors(IEnumerable<ValidationError> validationErrors, string indent = "")
            {
                foreach (var e in validationErrors)
                {
                    erreur.Append($"{Environment.NewLine}{indent}[{e.LinePosition}]: {e.Kind} - {e.Path}");
                    if (e is ChildSchemaValidationError csve)
                    {
                        foreach (var schema in csve.Errors)
                        {
                            var newIndent = indent + "  ";
                            if (csve.Errors.Count > 1)
                            {
                                erreur.Append($"{Environment.NewLine}{newIndent}{schema.Key.Description}");
                                newIndent += "  ";
                            }

                            HandleErrors(schema.Value, newIndent);
                        }
                    }
                }
            }

            HandleErrors(errors);
            throw new ModelException(erreur.ToString());
        }
    }

    private void CheckCore(JsonSchema schema, string fileName, string? content = null)
    {
        content ??= File.ReadAllText(fileName);

        var parser = new Parser(new StringReader(content));
        parser.Consume<StreamStart>();

        var firstObject = true;
        while (parser.Current is DocumentStart)
        {
            var yaml = _deserializer.Deserialize(parser)
                ?? throw new ModelException($"Impossible de lire le fichier {fileName.ToRelative()}.");
            var json = _serializer.Serialize(yaml);

            var finalSchema = firstObject && schema.OneOf.Any() ? schema.OneOf.First() : schema;

            Validate(fileName, finalSchema, json);

            firstObject = false;
        }
    }
}
