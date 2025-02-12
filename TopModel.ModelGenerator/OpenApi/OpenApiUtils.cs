using Microsoft.OpenApi.Models;
using TopModel.Utils;

namespace TopModel.ModelGenerator.OpenApi;

public static class OpenApiUtils
{
    public static string Format(this string? description)
    {
        if (description == null)
        {
            return string.Empty;
        }

        if (description.Contains('"') || description.Contains('\n') || description.Contains(':') || description.Contains('#'))
        {
            var lines = description.ReplaceLineEndings().Split(Environment.NewLine);
            if (string.IsNullOrWhiteSpace(lines.Last()))
            {
                lines = lines.SkipLast(1).ToArray();
            }

            return $"|{Environment.NewLine}{string.Join(Environment.NewLine, lines.Select(line => $"        {line}"))}";
        }
        else
        {
            return description;
        }
    }

    public static (string? Kind, string? Name) GetComposition(this OpenApiDocument model, OpenApiSchema schema)
    {
        if (schema.AnyOf.Any() || schema.OneOf.Any())
        {
            return ("object", schema.Reference.Id);
        }

        return schema.Items?.Reference != null
            ? ("list", schema.Items.Reference.Id)
            : schema.Reference != null && model.GetSchemas().Any(s => s.Value.Reference == schema.Reference)
            ? model.GetSchemas().First(s => s.Value.Reference == schema.Reference).Value.Type == "array"
                ? ("list", schema.Reference.Id.Unplurialize())
                : ("object", schema.Reference.Id)
            : schema.Type == "object" && schema.AdditionalProperties?.Reference != null
            ? ("map", schema.AdditionalProperties.Reference.Id)
            : schema.Type == "object" && schema.AdditionalProperties?.Items?.Reference != null
            ? ("list-map", schema.AdditionalProperties.Items.Reference.Id)
            : (null, null);
    }

    public static string GetDomain(this OpenApiConfig config, string name, OpenApiSchema schema)
    {
        var resolvedDomain = TmdGenUtils.GetDomainString(config.Domains, name: name);
        if (resolvedDomain == name)
        {
            return GetDomainSchema(config, schema);
        }

        return resolvedDomain;
    }

    public static string GetOperationId(this OpenApiDocument model, KeyValuePair<OperationType, OpenApiOperation> operation)
    {
        if (operation.Value.OperationId != null)
        {
            return operation.Value.OperationId;
        }

        var path = model.GetOperationPath(operation.Value).Replace("api/", string.Empty).Trim('/');

        if (!path.Contains('/'))
        {
            return path.ToPascalCase();
        }

        var id = operation.Key.ToString().ToPascalCase();

        if (operation.Key == OperationType.Get || operation.Key == OperationType.Head)
        {
            var responseSchema = model.GetResponseSchema(operation.Value);
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
                var (kind, name) = model.GetComposition(bodySchema);
                id += name;

                if (kind != null && kind != "object")
                {
                    id += kind.ToPascalCase();
                }
            }
        }

        return id;
    }

    public static string GetOperationPath(this OpenApiDocument model, OpenApiOperation operation)
    {
        return model.Paths.Single(p => p.Value.Operations.Any(o => o.Value == operation)).Key[1..];
    }

    public static IDictionary<string, OpenApiSchema> GetProperties(this OpenApiSchema schema)
    {
        if (schema.Type == "array")
        {
            return schema.Items.GetProperties();
        }

        return schema.Properties
            .Concat(schema.AllOf.Where(a => a.Type == "object").SelectMany(a => a.Properties))
            .ToDictionary(a => a.Key, a => a.Value);
    }

    public static OpenApiSchema? GetRequestBodySchema(this OpenApiOperation operation)
    {
        var schema = operation.RequestBody?.Content.First().Value.Schema;
        if (schema != null && schema.Reference == null)
        {
            schema.Reference = operation.RequestBody?.Reference;
        }

        return schema;
    }

    public static KeyValuePair<string, OpenApiSchema> GetResponseSchema(this OpenApiDocument model, OpenApiOperation operation)
    {
        var response = operation.Responses.FirstOrDefault(r => r.Key == "200" || r.Key == "201").Value;
        if (response != null && response.Content.Any())
        {
            return new(model.Components.Schemas.FirstOrDefault(s => s.Value == response.Content.First().Value.Schema).Key, response.Content.First().Value.Schema);
        }

        return default;
    }

    public static IDictionary<string, OpenApiSchema> GetSchemas(this OpenApiDocument model, HashSet<string>? references = null)
    {
        var schemas = model.Components.Schemas;
        foreach (var s in model.Components.RequestBodies.ToDictionary(r => r.Key, r =>
        {
            var schema = r.Value.Content.First().Value.Schema;
            schema.Reference ??= r.Value.Reference;

            return schema;
        }))
        {
            if (!schemas.ContainsKey(s.Key))
            {
                schemas.Add(s);
            }
        }

        foreach (var s in model.Components.Responses.Where(r => r.Value.Content.Any()).ToDictionary(r => r.Key, r =>
        {
            var schema = r.Value.Content.First().Value.Schema;
            schema.Reference ??= r.Value.Reference;

            return schema;
        }))
        {
            if (!schemas.ContainsKey(s.Key))
            {
                schemas.Add(s);
            }
        }

        foreach (var s in model.Paths
            .SelectMany(p => p.Value.Operations.Where(o => o.Value.Tags.Any()))
            .Where(o => o.Value.RequestBody != null)
            .ToDictionary(r => $"{r.Key}{model.GetOperationId(r)}Body", r => r.Value.RequestBody.Content.First().Value.Schema))
        {
            if (!schemas.ContainsKey(s.Key))
            {
                schemas.Add(s);
            }
        }

        return schemas.Where(s =>
             s.Value.Type == "object"
             || s.Value.AllOf.Any() && s.Value.AllOf.All(a => a.Type == "object" || a.Reference != null)
             || s.Value.Type == "array" && s.Value.Items.Type == "object"
             || s.Value.AnyOf.Any()
             || s.Value.OneOf.Any()
             || s.Value.Type == "string" && s.Value.Enum.Any())
         .Where(s => references == null || references.Contains(s.Key))
         .ToDictionary(a => a.Key, a => a.Value);
    }

    public static string Unplurialize(this string name)
    {
        return name.EndsWith("ies") ? $"{name[..^3]}y" : name.TrimEnd('s');
    }

    private static string GetDomainCore(this OpenApiSchema schema)
    {
        var length = schema.MaxLength != null ? $"{schema.MaxLength}" : string.Empty;

        if (schema.Format != null)
        {
            return schema.Format + length;
        }
        else if (schema.Type == "array")
        {
            return $"{GetDomainCore(schema.Items)}-array";
        }
        else if (schema.Type == "object" && schema.AdditionalProperties != null)
        {
            return $"{GetDomainCore(schema.AdditionalProperties)}-map";
        }

        return schema.Type + length;
    }

    private static string GetDomainSchema(this OpenApiConfig config, OpenApiSchema schema)
    {
        var domain = GetDomainCore(schema);
        return TmdGenUtils.GetDomainString(config.Domains, type: domain);
    }
}
