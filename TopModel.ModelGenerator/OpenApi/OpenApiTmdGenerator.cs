using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using TopModel.Utils;

namespace TopModel.ModelGenerator.OpenApi;

public class OpenApiTmdGenerator : ModelGenerator
{
    private readonly OpenApiConfig _config;
    private readonly ILogger<OpenApiTmdGenerator> _logger;
    private readonly IFileWriterProvider _writerProvider;

#nullable disable
    private OpenApiDocument _model;
#nullable enable

    public OpenApiTmdGenerator(ILogger<OpenApiTmdGenerator> logger, OpenApiConfig config, IFileWriterProvider writerProvider)
        : base(logger)
    {
        _config = config;
        _logger = logger;
        _writerProvider = writerProvider;

        if (_config.ModelTags.Count == 0)
        {
            _config.ModelTags.Add("OpenApi");
        }

        if (_config.EndpointTags.Count == 0)
        {
            _config.EndpointTags.Add("OpenApi");
        }
    }

    public override string Name => "OpenApiGen";

    protected override async IAsyncEnumerable<string> GenerateCore()
    {
        var modelReader = new OpenApiStreamReader();
        if (_config.Source.StartsWith("http://") || _config.Source.StartsWith("https://"))
        {
            using var client = new HttpClient();
            var openApi = await client.GetAsync(_config.Source);
            _model = modelReader.Read(await openApi.Content.ReadAsStreamAsync(), out var diagnostic);
        }
        else
        {
            using var stream = File.Open(DirectoryName + "/" + _config.Source, FileMode.Open);
            _model = modelReader.Read(stream, out var diagnostic);
        }

        var modules = _model.Paths
            .SelectMany(p => p.Value.Operations.Where(o => o.Value.Tags.Any()))
            .GroupBy(o => o.Value.Tags.First().Name.ToPascalCase())
            .Where(m => m.Key != "Null" && (_config.Include == null || _config.Include.Select(i => i.ToPascalCase()).Contains(m.Key)));

        var referenceMap = modules.ToDictionary(m => m.Key, GetModuleReferences);

        var modelFileName = $"{Path.Combine(ModelRoot, _config.OutputDirectory, _config.ModelFileName)}.tmd";
        yield return modelFileName;

        var rootPath = Path.Combine(ModelRoot, _config.OutputDirectory);
        var tmdFile = new TmdFile()
        {
            Module = _config.Module,
            Name = _config.ModelFileName,
            Tags = _config.ModelTags.ToList(),
            Path = Path.Combine(_config.OutputDirectory, _config.ModelFileName)
        };
        var fileName = Path.Combine(rootPath, tmdFile.Name + ".tmd");

        using var fileWriter = _writerProvider.OpenFileWriter(modelFileName, _logger);
        using var tmdFileWriter = new TmdWriter(fileWriter, tmdFile, Path.GetFullPath(ModelRoot));

        var references = new HashSet<string>(referenceMap.SelectMany(r => r.Value).Select(r => r.Id).Distinct());

        foreach (var schema in _model.GetSchemas(references).OrderBy(r => r.Key))
        {
            if (schema.Value.Type == "string" && schema.Value.Enum.Any())
            {
                var classes = tmdFile.Classes.Select(c => c.Name);
                if (classes.Contains($"{_config.ClassPrefix}{schema.Key.ToPascalCase()}"))
                {
                    continue;
                }

                var enumClass = new TmdClass()
                {
                    File = tmdFile,
                    Name = $"{_config.ClassPrefix}{schema.Key.ToPascalCase()}",
                    Comment = $"enum pour les valeurs de {schema.Key.ToPascalCase()}",
                    PreservePropertyCasing = _config.PreservePropertyCasing
                };

                tmdFile.Classes.Add(enumClass);

                var p = WriteProperty(_config, new("Value", schema.Value), tmdFile);
                p.Class = enumClass;
                enumClass.Properties.Add(p);
                AddValues(enumClass, schema.Value);
            }
            else
            {
                var classe = new TmdClass()
                {
                    File = tmdFile,
                    PreservePropertyCasing = _config.PreservePropertyCasing
                };

                var className = schema.Value.Type == "array" ? schema.Key.Unplurialize() : schema.Key;
                classe.Name = $"{_config.ClassPrefix}{className.ToPascalCase()}";
                var classes = tmdFile.Classes.Select(c => c.Name);
                if (classes.Contains(classe.Name))
                {
                    continue;
                }

                var parents = _model.GetSchemas().Where(s => s.Value.AnyOf.Contains(schema.Value) || s.Value.OneOf.Contains(schema.Value));
                if (parents.Count() == 1)
                {
                    classe.Extends = parents.Single().Key.ToPascalCase();
                }

                tmdFile.Classes.Add(classe);

                if (!string.IsNullOrEmpty(schema.Value.Description?.Trim(' ')))
                {
                    classe.Comment = @$"{schema.Value.Description.Format()}";
                }

                var classeProperties = classe.Properties;
                var properties = schema.Value.GetProperties().ToList();

                foreach (var property in properties)
                {
                    if (!property.Value.Enum.Any())
                    {
                        var p = WriteProperty(_config, property, tmdFile);
                        p.Class = classe;
                        classeProperties.Add(p);
                    }
                    else
                    {
                        var enumClass = tmdFile.Classes.Where(c => c.Name == $"{classe.Name}{property.Key.ToPascalCase()}").SingleOrDefault();
                        if (enumClass == null)
                        {
                            enumClass = new TmdClass()
                            {
                                File = tmdFile,
                                Name = $"{classe.Name}{property.Key.ToPascalCase()}",
                                Comment = $"enum pour les valeurs de {property.Key.ToPascalCase()}",
                                PreservePropertyCasing = _config.PreservePropertyCasing
                            };

                            tmdFile.Classes.Add(enumClass);
                            var p = WriteProperty(_config, new("Value", property.Value), tmdFile);
                            p.Class = enumClass;
                            enumClass.Properties.Add(p);
                            AddValues(enumClass, property.Value);
                        }

                        classeProperties.Add(new TmdAliasProperty()
                        {
                            Alias = enumClass.Properties[0],
                            Name = $"{property.Key}",
                            Comment = @$"{property.Value.Description.Format()}",
                            Class = classe
                        });
                    }
                }
            }
        }

        foreach (var cp in tmdFile.Classes.SelectMany(c => c.Properties.OfType<TmdCompositionProperty>()))
        {
            var composition = tmdFile.Classes.Where(c => c.Name == cp.CompositionReference).FirstOrDefault();
            cp.Composition = composition;
        }

        foreach (var module in modules)
        {
            var endpointFileName = $"{Path.Combine(ModelRoot, _config.OutputDirectory, module.Key)}.tmd";
            yield return endpointFileName;

            var tmdFileEnpoint = new TmdFile()
            {
                Module = _config.Module,
                Name = module.Key,
                Tags = _config.EndpointTags.ToList(),
                Path = Path.Combine(_config.OutputDirectory, module.Key)
            };

            using var endpointFileWriter = _writerProvider.OpenFileWriter(endpointFileName, _logger);
            using var tmdEndpointFileWriter = new TmdWriter(endpointFileWriter, tmdFileEnpoint, ModelRoot);

            if (referenceMap[module.Key].Any())
            {
                var use = $"{_config.OutputDirectory.Replace("\\", "/")}/{_config.ModelFileName}".Replace("//", "/");
                if (use.StartsWith("./"))
                {
                    use = use.Replace("./", string.Empty);
                }
            }

            foreach (var operation in module.OrderBy(o => GetEndpointName(o)))
            {
                var path = GetOperationPath(operation.Value);
                var endPoint = new TmdEndpoint()
                {
                    Name = GetEndpointName(operation),
                    Method = operation.Key.ToString().ToUpper(),
                    Route = path,
                    File = tmdFileEnpoint
                };
                tmdFileEnpoint.Endpoints.Add(endPoint);
                if (!string.IsNullOrEmpty(operation.Value.Summary))
                {
                    endPoint.Comment = @$"""{operation.Value.Summary}""";
                }

                endPoint.PreservePropertyCasing = _config.PreservePropertyCasing;

                if (operation.Value.Parameters.Any(p => p.In == ParameterLocation.Query || p.In == ParameterLocation.Path) || operation.Value.RequestBody != null)
                {
                    var bodySchema = operation.Value.GetRequestBodySchema();
                    if (bodySchema != null)
                    {
                        var p = WriteProperty(_config, new("body", bodySchema), tmdFile);
                        if (p is TmdCompositionProperty cp)
                        {
                            cp.Composition = tmdFile.Classes.Where(c => c.Name == cp.CompositionReference).SingleOrDefault();
                        }

                        endPoint.Params.Add(p);
                    }

                    foreach (var param in operation.Value.Parameters.Where(p => p.In == ParameterLocation.Query || p.In == ParameterLocation.Path).OrderBy(p => path.Contains($@"{{{p.Name}}}") ? 0 + p.Name : 1 + p.Name))
                    {
                        TmdProperty property;
                        if (param.Schema.Enum.Any())
                        {
                            var enumClass = tmdFile.Classes.Where(c => c.Name == $"{endPoint.Name.ToPascalCase()}{param.Name.ToPascalCase()}").SingleOrDefault();
                            if (enumClass == null)
                            {
                                enumClass = new TmdClass()
                                {
                                    File = tmdFile,
                                    Name = $"{endPoint.Name.ToPascalCase()}{param.Name.ToPascalCase()}",
                                    Comment = $"enum pour les valeurs de {param.Name}",
                                    PreservePropertyCasing = _config.PreservePropertyCasing
                                };

                                tmdFile.Classes.Add(enumClass);
                                var p = WriteProperty(_config, new("Value", param.Schema), tmdFile);
                                p.Class = enumClass;
                                enumClass.Properties.Add(p);
                                AddValues(enumClass, param.Schema);
                            }

                            property = new TmdAliasProperty()
                            {
                                Alias = enumClass.Properties[0],
                                Name = $"{param.Name.ToPascalCase()}",
                                Class = enumClass
                            };
                        }
                        else
                        {
                            property = new TmdRegularProperty()
                            {
                                Name = param.Name,
                                Domain = _config.GetDomain(param.Name, param.Schema)
                            };
                        }

                        endPoint.Params.Add(property);
                        if (!string.IsNullOrEmpty(param.Description?.Trim(' ')))
                        {
                            property.Comment = $@"{param.Description.Format()}";
                        }
                    }
                }

                var responseSchema = GetResponseSchema(operation.Value).Value;
                if (responseSchema != null)
                {
                    var returns = WriteProperty(_config, new("Result", responseSchema), tmdFile);
                    if (returns is TmdCompositionProperty cp)
                    {
                        cp.Composition = tmdFile.Classes.Where(c => c.Name == cp.CompositionReference).SingleOrDefault();
                    }

                    endPoint.Returns = returns;
                }
            }
        }
    }

    private static void AddValues(TmdClass classe, OpenApiSchema schema)
    {
        foreach (var val in schema.Enum.OfType<OpenApiString>())
        {
            Dictionary<string, string?> value = new()
                {
                    { classe.Properties.First().Name, val.Value }
                };
            classe.Values.Add(value);
        }

        foreach (var val in schema.Enum.OfType<OpenApiInteger>())
        {
            Dictionary<string, string?> value = new()
                {
                    { classe.Properties.First().Name, $"{val.Value}" }
                };
            classe.Values.Add(value);
        }

        foreach (var val in schema.Enum.OfType<OpenApiDouble>())
        {
            Dictionary<string, string?> value = new()
                {
                    { classe.Properties.First().Name, $"{val.Value}" }
                };
            classe.Values.Add(value);
        }

        foreach (var val in schema.Enum.OfType<OpenApiDate>())
        {
            Dictionary<string, string?> value = new()
                {
                    { classe.Properties.First().Name, $"{val.Value}" }
                };
            classe.Values.Add(value);
        }

        foreach (var val in schema.Enum.OfType<OpenApiBoolean>())
        {
            Dictionary<string, string?> value = new()
                {
                    { classe.Properties.First().Name, $"{val.Value}" }
                };
            classe.Values.Add(value);
        }
    }

    private (string? Kind, string? Name) GetComposition(OpenApiSchema schema)
    {
        if (schema.AnyOf.Any() || schema.OneOf.Any())
        {
            return ("object", schema.Reference.Id);
        }

        return schema.Items?.Reference != null
            ? ("list", schema.Items.Reference.Id)
            : schema.Reference != null && _model.GetSchemas().Any(s => s.Value.Reference == schema.Reference)
            ? _model.GetSchemas().First(s => s.Value.Reference == schema.Reference).Value.Type == "array"
                ? ("list", schema.Reference.Id.Unplurialize())
                : ("object", schema.Reference.Id)
            : schema.Type == "object" && schema.AdditionalProperties?.Reference != null
            ? ("map", schema.AdditionalProperties.Reference.Id)
            : schema.Type == "object" && schema.AdditionalProperties?.Items?.Reference != null
            ? ("list-map", schema.AdditionalProperties.Items.Reference.Id)
            : (null, null);
    }

    private string GetEndpointName(KeyValuePair<OperationType, OpenApiOperation> operation)
    {
        var operationId = GetOperationId(operation);
        var operationsWithId = _model!.Paths.OrderBy(p => p.Key).SelectMany(p => p.Value.Operations.OrderBy(o => o.Key))
            .Where(o => GetOperationId(o) == operationId)
            .ToList();

        if (operationsWithId.Count == 1)
        {
            return operationId;
        }

        var prefix = operationsWithId.DistinctBy(o => o.Key).Count() > 1
            ? operation.Key.ToString().ToPascalCase()
            : string.Empty;

        var suffix = string.Empty;

        if (prefix == string.Empty)
        {
            suffix += operationsWithId.IndexOf(operation) + 1;
        }
        else
        {
            var operationWithIdAndMethod = operationsWithId.Where(o => o.Key == operation.Key).ToList();
            if (operationWithIdAndMethod.Count > 1)
            {
                suffix += operationWithIdAndMethod.IndexOf(operation) + 1;
            }
        }

        return $"{prefix}{operationId}{suffix}";
    }

    private IEnumerable<OpenApiReference> GetModuleReferences(IEnumerable<KeyValuePair<OperationType, OpenApiOperation>> operations)
    {
        var visited = new HashSet<OpenApiSchema>();

        foreach (var operation in operations)
        {
            if (operation.Value.RequestBody != null)
            {
                foreach (var reference in GetSchemaReferences(operation.Value.RequestBody.Content.First().Value.Schema, visited))
                {
                    yield return reference;
                }

                if (operation.Value.RequestBody.Reference == null
                    && operation.Value.RequestBody.Content.First().Value.Schema.Reference == null
                    && operation.Value.RequestBody.Content.First().Value.Schema.Type == "object")
                {
                    operation.Value.RequestBody.Reference = new OpenApiReference()
                    {
                        Id = $"{operation.Value.OperationId}Body"
                    };
                }

                if (operation.Value.RequestBody.Reference != null)
                {
                    yield return operation.Value.RequestBody.Reference;
                }
            }

            foreach (var reference in operation.Value.Parameters.SelectMany(p => GetSchemaReferences(p.Schema, visited)))
            {
                yield return reference;
            }

            var response = operation.Value.Responses.FirstOrDefault(r => r.Key == "200" || r.Key == "201").Value;
            if (response != null && response.Content.Any())
            {
                foreach (var reference in GetSchemaReferences(response.Content.First().Value.Schema, visited))
                {
                    yield return reference;
                }

                if (response.Reference != null)
                {
                    yield return response.Reference;
                }
            }
        }
    }

    private string GetOperationId(KeyValuePair<OperationType, OpenApiOperation> operation)
    {
        if (operation.Value.OperationId != null)
        {
            return operation.Value.OperationId;
        }

        var path = GetOperationPath(operation.Value).Replace("api/", string.Empty).Trim('/');

        if (!path.Contains('/'))
        {
            return path.ToPascalCase();
        }

        var id = operation.Key.ToString().ToPascalCase();

        if (operation.Key == OperationType.Get || operation.Key == OperationType.Head)
        {
            var responseSchema = GetResponseSchema(operation.Value);
            if (responseSchema.Key != null)
            {
                id += responseSchema.Key;
            }
        }
        else
        {
            var bodySchema = operation.Value.GetRequestBodySchema();
            if (bodySchema != null)
            {
                var body = GetComposition(bodySchema);
                id += body.Name;

                if (body.Kind != null && body.Kind != "object")
                {
                    id += body.Kind.ToPascalCase();
                }
            }
        }

        return id;
    }

    private string GetOperationPath(OpenApiOperation operation)
    {
        return _model.Paths.Single(p => p.Value.Operations.Any(o => o.Value == operation)).Key[1..];
    }

    private KeyValuePair<string, OpenApiSchema> GetResponseSchema(OpenApiOperation operation)
    {
        var response = operation.Responses.FirstOrDefault(r => r.Key == "200" || r.Key == "201").Value;
        if (response != null && response.Content.Any())
        {
            return new(_model.Components.Schemas.FirstOrDefault(s => s.Value == response.Content.First().Value.Schema).Key, response.Content.First().Value.Schema);
        }

        return default;
    }

    private IEnumerable<OpenApiReference> GetSchemaReferences(OpenApiSchema schema, HashSet<OpenApiSchema> visited)
    {
        visited.Add(schema);

        if (schema.Reference != null)
        {
            yield return schema.Reference;
        }

        if (schema.Items != null && !visited.Contains(schema.Items))
        {
            foreach (var reference in GetSchemaReferences(schema.Items, visited))
            {
                yield return reference;
            }
        }

        foreach (var reference in schema.GetProperties().Values.Where(p => !visited.Contains(p)).SelectMany(p => GetSchemaReferences(p, visited)))
        {
            yield return reference;
        }

        if (schema.AdditionalProperties != null && !visited.Contains(schema.AdditionalProperties))
        {
            foreach (var reference in GetSchemaReferences(schema.AdditionalProperties, visited))
            {
                yield return reference;
            }
        }

        foreach (var oneOff in schema.OneOf)
        {
            foreach (var reference in GetSchemaReferences(oneOff, visited))
            {
                yield return reference;
            }
        }

        foreach (var anyOff in schema.AnyOf)
        {
            foreach (var reference in GetSchemaReferences(anyOff, visited))
            {
                yield return reference;
            }
        }
    }

    private TmdProperty WriteProperty(OpenApiConfig config, KeyValuePair<string, OpenApiSchema> property, TmdFile tmdFile)
    {
        var (kind, name) = GetComposition(property.Value);
        if (property.Value.Type == "array" && property.Value.Items.Enum.Any() && property.Value.Items.Type == "string" && property.Value.Items.Reference != null)
        {
            var aliasClass = tmdFile.Classes.Where(c => c.Name == property.Value.Items.Reference.Id.ToPascalCase()).SingleOrDefault();
            if (aliasClass == null)
            {
                aliasClass = new TmdClass()
                {
                    File = tmdFile,
                    Name = $"{_config.ClassPrefix}{property.Value.Items.Reference.Id.ToPascalCase()}",
                    Comment = $"enum pour les valeurs de {property.Value.Items.Reference.Id.ToPascalCase()}",
                    PreservePropertyCasing = _config.PreservePropertyCasing
                };

                tmdFile.Classes.Add(aliasClass);

                var p = WriteProperty(_config, new("Value", property.Value.Items), tmdFile);
                p.Class = aliasClass;
                aliasClass.Properties.Add(p);
                AddValues(aliasClass, property.Value.Items);
            }

            var aliasProperty = new TmdAliasProperty()
            {
                Name = $"{property.Key}",
                Required = !property.Value.Nullable,
                Domain = $"{config.GetDomain(property.Key, property.Value)}",
                Alias = aliasClass.Properties.First(),
                As = "list"
            };

            if (!string.IsNullOrEmpty(property.Value.Description?.Trim(' ')))
            {
                aliasProperty.Comment = $"{property.Value.Description.Format()}";
            }

            return aliasProperty;
        }
        else if (!(property.Value.Type == "string" && property.Value.Enum.Any())
                && kind != null && name != null)
        {
            var compositionProperty = new TmdCompositionProperty()
            {
                Name = $"{property.Key}",
                Required = !property.Value.Nullable,
            };
            var domainKind = TmdGenUtils.GetDomainString(config.Domains, type: kind);
            if (kind != "object")
            {
                compositionProperty.Domain = $"{domainKind ?? kind}";
            }

            if (!string.IsNullOrEmpty(property.Value.Description?.Trim(' ')))
            {
                compositionProperty.Comment = $"{property.Value.Description.Format()}";
            }

            compositionProperty.CompositionReference = $"{_config.ClassPrefix}{name.ToPascalCase()}";

            return compositionProperty;
        }
        else
        {
            var regularProperty = new TmdRegularProperty()
            {
                Name = $"{property.Key}",
                Required = !property.Value.Nullable,
                Domain = $"{config.GetDomain(property.Key, property.Value)}"
            };

            if (!string.IsNullOrEmpty(property.Value.Description?.Trim(' ')))
            {
                regularProperty.Comment = $"{property.Value.Description.Format()}";
            }

            return regularProperty;
        }
    }
}
