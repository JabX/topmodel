using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

using static JavascriptUtils;

/// <summary>
/// Générateur de définitions Typescript.
/// </summary>
public class TypescriptDefinitionGenerator(ILogger<TypescriptDefinitionGenerator> logger, GeneratedFileWriterProvider writerProvider)
    : ClassGeneratorBase<JavascriptConfig>(logger, writerProvider)
{
    public override string Name => "JSDefinitionGen";

    protected override bool FilterClass(Class classe)
    {
        return !classe.IsJSReference();
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Config.GetClassFileName(classe, tag);
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        using var fw = OpenFileWriter(fileName, false);

        if (Config.EntityMode == EntityMode.TYPED)
        {
            fw.WriteLine($"import {{{string.Join(", ", GetFocusStoresImports(fileName, classe, tag).OrderBy(x => x))}}} from \"{Config.GetRelativePath(Config.EntityTypesPath, fileName)}\";");
        }

        if ((Config.EntityMode == EntityMode.TYPED || Config.EntityMode == EntityMode.UNTYPED) && classe.Properties.Any(c => c.Domain is not null && !Config.IsListComposition(c)))
        {
            var domainImport = Config.GetRelativePath(Config.ResolveVariables(Config.DomainPath, tag), fileName);
            fw.WriteLine($"import {{{string.Join(", ", classe.Properties
                .Select(p => p is not CompositionProperty and not AliasProperty { Property: CompositionProperty }
                    ? p.Domain
                    : p is CompositionProperty or AliasProperty { Property: CompositionProperty } && !Config.IsListComposition(p)
                        ? p.Domain
                        : null!)
                .Where(d => d != null)
                .OrderBy(d => d.Name)
                .Select(d => d.Name).Distinct())}}} from \"{domainImport}\";");
        }

        var imports = classe.ClassDependencies
            .Select(dep => (
                Import: (dep is { Source: CompositionProperty { Domain: not null } cp } && !Config.IsListComposition(cp)) || (dep is { Source: AliasProperty { Property: CompositionProperty { Domain: not null } cp2 } } && !Config.IsListComposition(cp2))
                    ? dep.Classe.NamePascal
                    : dep is { Source: IProperty fp and not CompositionProperty and not AliasProperty { Property: CompositionProperty } }
                    ? Config.GetEnumType(fp)
                    : $"{(Config.EntityMode == EntityMode.TYPED || Config.EntityMode == EntityMode.UNTYPED ? dep.Classe.NamePascal + "Entity, " : string.Empty)}{dep.Classe.NamePascal}{(Config.EntityMode == EntityMode.TYPED ? "EntityType" : string.Empty)}",
                Path: Config.GetImportPathForClass(dep, dep.Classe.Tags.Contains(tag) ? tag : dep.Classe.Tags.Intersect(Config.Tags).FirstOrDefault() ?? tag, tag, Classes)!))
            .Concat(classe.Properties.SelectMany(dep => Config.GetDomainImportPaths(fileName, dep, tag)))
            .Concat(classe.Properties.SelectMany(dep => Config.GetValueImportPaths(fileName, dep)))
            .Where(p => p.Path != null && p.Path != Config.EntityTypesPath)
            .GroupAndSort();

        fw.WriteLine();

        foreach (var import in imports)
        {
            fw.WriteLine($"import {{{import.Import}}} from \"{import.Path}\";");
        }

        if (imports.Count > 0)
        {
            fw.WriteLine();
        }

        if (Config.EntityMode == EntityMode.TYPED)
        {
            fw.WriteLine($"export type {classe.NamePascal} = EntityToType<{classe.NamePascal}EntityType>;");

            fw.Write($"export interface {classe.NamePascal}EntityType ");

            if (classe.Extends != null)
            {
                fw.Write($"extends {classe.Extends.NamePascal}EntityType ");
            }
        }
        else
        {
            fw.Write("export interface ");
            fw.Write($"{classe.NamePascal} ");

            if (classe.Extends != null)
            {
                fw.Write($"extends {classe.Extends.NamePascal} ");
            }
        }

        fw.Write("{\r\n");

        foreach (var property in classe.Properties)
        {
            fw.Write($"    {property.NameCamel}{(Config.EntityMode == EntityMode.TYPED ? string.Empty : "?")}: ");

            if (Config.EntityMode == EntityMode.TYPED)
            {
                switch (property)
                {
                    case CompositionProperty { Domain: null } cp:
                        fw.Write($"ObjectEntry<{cp.Composition.NamePascal}EntityType>;");
                        break;
                    case AliasProperty { Property: CompositionProperty { Domain: null } cp }:
                        fw.Write($"ObjectEntry<{cp.Composition.NamePascal}EntityType>;");
                        break;
                    case CompositionProperty cp when Config.IsListComposition(cp):
                        if (cp.Composition.Name == classe.Name)
                        {
                            fw.Write($"RecursiveListEntry");
                        }
                        else
                        {
                            fw.Write($"ListEntry<{cp.Composition.NamePascal}EntityType>;");
                        }

                        break;
                    case AliasProperty { Property: CompositionProperty cp } when Config.IsListComposition(cp):
                        if (cp.Composition.Name == classe.Name)
                        {
                            fw.Write($"RecursiveListEntry;");
                        }
                        else
                        {
                            fw.Write($"ListEntry<{cp.Composition.NamePascal}EntityType>;");
                        }

                        break;
                    default:
                        fw.Write($"FieldEntry2<typeof {property.Domain.Name}, {Config.GetType(property, Classes)}>;");
                        break;
                }
            }
            else
            {
                fw.Write($"{Config.GetType(property, Classes)};");
            }

            fw.Write("\r\n");
        }

        fw.Write("}\r\n\r\n");

        if (Config.EntityMode == EntityMode.TYPED || Config.EntityMode == EntityMode.UNTYPED)
        {
            fw.Write($"export const {classe.NamePascal}Entity");

            if (Config.EntityMode == EntityMode.TYPED)
            {
                fw.Write($": {classe.NamePascal}EntityType");
            }

            fw.Write(" = {\r\n");

            if (classe.Extends != null)
            {
                fw.Write("    ...");
                fw.Write(classe.Extends.NamePascal);
                fw.Write("Entity,\r\n");
            }

            foreach (var property in classe.Properties)
            {
                fw.Write("    ");
                fw.Write(property.NameCamel);
                fw.Write(": {\r\n");
                fw.Write("        type: ");

                switch (property)
                {
                    case CompositionProperty { Domain: null }:
                    case AliasProperty { Property: CompositionProperty { Domain: null } }:
                        fw.Write("\"object\",");
                        break;
                    case CompositionProperty cp1 when Config.IsListComposition(cp1) && cp1.Composition.Name == classe.Name:
                    case AliasProperty { Property: CompositionProperty cp2 } when Config.IsListComposition(cp2) && cp2.Composition.Name == classe.Name:
                        fw.Write("\"recursive-list\"");
                        if (Config.ExtendedCompositions)
                        {
                            fw.Write(",");
                        }

                        break;
                    case CompositionProperty when Config.IsListComposition(property):
                    case AliasProperty { Property: CompositionProperty } when Config.IsListComposition(property):
                        fw.Write("\"list\",");
                        break;
                    default:
                        fw.Write("\"field\",");
                        break;
                }

                fw.Write("\r\n");

                var cp = property switch
                {
                    CompositionProperty c => c,
                    AliasProperty { Property: CompositionProperty c } => c,
                    _ => null
                };

                if (cp == null || cp.Domain != null && !Config.IsListComposition(cp))
                {
                    fw.WriteLine(2, $"name: \"{property.NameCamel}\",");
                    fw.WriteLine(2, $"domain: {property.Domain.Name},");

                    var defaultValue = Config.GetValue(property, Classes);
                    if (defaultValue != "undefined")
                    {
                        fw.WriteLine(2, $"defaultValue: {defaultValue},");
                    }
                }
                else if (cp.Composition.Name != classe.Name)
                {
                    fw.Write(2, $"entity: {cp.Composition.NamePascal}Entity");

                    if (Config.ExtendedCompositions)
                    {
                        fw.Write(",");
                    }

                    fw.WriteLine();
                }

                if (cp == null || cp.Domain != null && !Config.IsListComposition(cp) || Config.ExtendedCompositions)
                {
                    fw.WriteLine(2, $"isRequired: {(property.Required && !((property.PrimaryKey || property is AliasProperty { AliasedPrimaryKey: true }) && property.Domain.AutoGeneratedValue)).ToString().ToFirstLower()},");

                    if (Config.TranslateProperties == true)
                    {
                        fw.WriteLine(2, $"label: \"{property.ResourceKey}\"{(Config.GenerateComments ? "," : string.Empty)}");
                    }
                    else
                    {
                        fw.WriteLine(2, $"label: \"{property.Label}\"{(Config.GenerateComments ? "," : string.Empty)}");
                    }

                    if (Config.GenerateComments)
                    {
                        if (Config.TranslateProperties == true)
                        {
                            fw.WriteLine(2, $"comment: \"{property.CommentResourceKey}\"");
                        }
                        else
                        {
                            fw.WriteLine(2, $"comment: \"{property.Comment}\"");
                        }
                    }
                }

                fw.Write(1, "}");

                if (property != classe.Properties.Last())
                {
                    fw.Write(",");
                }

                fw.WriteLine();
            }

            fw.WriteLine($"}}{(Config.EntityMode == EntityMode.TYPED ? string.Empty : " as const")};");
        }

        if (classe.Reference)
        {
            fw.WriteLine();
            WriteReferenceDefinition(fw, classe);
        }
    }

    private IEnumerable<string> GetFocusStoresImports(string fileName, Class classe, string tag)
    {
        if (classe.Properties.Any(p => p.Domain is not null && !Config.IsListComposition(p)))
        {
            yield return "FieldEntry2";
        }

        if (classe.Properties.Any(p => p is CompositionProperty { Domain: null } or AliasProperty { Property: CompositionProperty { Domain: null } }))
        {
            yield return "ObjectEntry";
        }

        if (classe.Properties.Any(p => (p is CompositionProperty && p.Class == classe && Config.IsListComposition(p)) || (p is AliasProperty { Property: CompositionProperty } && p.Class == classe && Config.IsListComposition(p))))
        {
            yield return "ListEntry";
        }

        if (classe.Properties.Any(p => (p is CompositionProperty cp && cp.Composition == classe && Config.IsListComposition(p)) || (p is AliasProperty { Property: CompositionProperty cp2 } && cp2.Composition == classe && Config.IsListComposition(p))))
        {
            yield return "RecursiveListEntry";
        }

        foreach (var p in classe.Properties.SelectMany(dep => Config.GetDomainImportPaths(fileName, dep, tag)).Where(p => p.Path == Config.EntityTypesPath))
        {
            yield return p.Import;
        }

        yield return "EntityToType";
    }
}