using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Javascript;

using static JavascriptUtils;

/// <summary>
/// Générateur de définitions Typescript.
/// </summary>
public class TypescriptReferenceGenerator(ILogger<TypescriptReferenceGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGroupGeneratorBase<JavascriptConfig>(logger, writerProvider)
{
    public override string Name => "JSReferenceGen";

    protected override IEnumerable<(string FileType, string FileName)> GetFileNames(Class classe, string tag)
    {
        if (classe.IsJSReference())
        {
            yield return ("main", Config.GetReferencesFileName(classe.Namespace, tag));
        }
    }

    protected override void HandleFile(string fileType, string fileName, string tag, IEnumerable<Class> classes)
    {
        GenerateReferenceFile(fileName, classes.OrderBy(r => r.NameCamel), tag);
    }

    /// <summary>
    /// Create the template output
    /// </summary>
    private void GenerateReferenceFile(string fileName, IEnumerable<Class> references, string tag)
    {
        using var fw = OpenFileWriter(fileName, false);

        var imports = references
            .SelectMany(r => r.ClassDependencies)
            .Select(dep => (
                Import: dep.Source switch
                {
                    IProperty fp => Config.GetType(fp, Classes),
                    Class c => c.NamePascal,
                    _ => null!
                },
                Path: Config.GetImportPathForClass(dep, dep.Classe.Tags.Contains(tag) ? tag : dep.Classe.Tags.Intersect(Config.Tags).FirstOrDefault() ?? tag, tag, Classes)!))
            .Concat(references.SelectMany(r => r.Properties).SelectMany(dep => Config.GetDomainImportPaths(fileName, dep, tag)))
            .Where(i => i.Path != null && i.Path != $"./references")
            .GroupAndSort();

        foreach (var import in imports)
        {
            fw.Write("import {");
            fw.Write(import.Import);
            fw.Write("} from \"");
            fw.Write(import.Path);
            fw.Write("\";\r\n");
        }

        if (imports.Count > 0)
        {
            fw.Write("\r\n");
        }

        var first = true;
        foreach (var reference in references)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                fw.WriteLine();
            }

            if (reference.EnumKey != null)
            {
                var values = GetAllValues(reference).ToList();

                if (reference.Extends == null)
                {
                    fw.Write("export type ");
                    fw.Write(reference.NamePascal);
                    fw.Write($"{reference.EnumKey.NamePascal} = ");
                    var type = Config.GetImplementation(reference.EnumKey.Domain)?.Type;
                    var quote = (type == "boolean" || type == "number") ? string.Empty : @"""";
                    fw.Write(string.Join(" | ", values.Select(r => $@"{quote}{r.Value[reference.EnumKey]}{quote}").OrderBy(x => x, StringComparer.Ordinal)));
                    fw.WriteLine(";");
                }

                foreach (var uk in reference.UniqueKeys.Where(uk => uk.Count == 1 && uk.Single().Required).Select(uk => uk.Single()))
                {
                    fw.Write("export type ");
                    fw.Write(reference.NamePascal);
                    fw.Write($"{uk} = ");
                    fw.Write(string.Join(" | ", values.Select(r => $@"""{r.Value[uk]}""").OrderBy(x => x, StringComparer.Ordinal)));
                    fw.WriteLine(";");
                }
            }

            if (reference.FlagProperty != null)
            {
                fw.Write($"export enum {reference.NamePascal}Flag {{\r\n");

                var flagValues = reference.Values.Where(refValue => refValue.Value.ContainsKey(reference.FlagProperty) && int.TryParse(refValue.Value[reference.FlagProperty], out var _)).ToList();
                foreach (var refValue in flagValues)
                {
                    var flag = int.Parse(refValue.Value[reference.FlagProperty]);
                    fw.Write($"    {refValue.Name} = 0b{Convert.ToString(flag, 2)}");
                    if (flagValues.IndexOf(refValue) != flagValues.Count - 1)
                    {
                        fw.WriteLine(",");
                    }
                }

                fw.WriteLine("\r\n}");
            }

            if (reference.Reference)
            {
                fw.Write("export interface ");
                fw.Write(reference.NamePascal);

                if (reference.Extends != null)
                {
                    fw.Write($" extends {reference.Extends.NamePascal}");
                }

                fw.Write(" {\r\n");

                foreach (var property in reference.Properties)
                {
                    fw.Write("    ");
                    fw.Write(property.NameCamel);
                    fw.Write(property.Required || property.PrimaryKey ? string.Empty : "?");
                    fw.Write(": ");
                    fw.Write(Config.GetType(property, Classes));
                    fw.Write(";\r\n");
                }

                fw.Write("}\r\n");

                if (Config.ReferenceMode == ReferenceMode.VALUES)
                {
                    WriteReferenceValues(fw, reference);
                }
                else
                {
                    WriteReferenceDefinition(fw, reference);
                }
            }
        }
    }

    private void WriteReferenceValues(IFileWriter fw, Class reference)
    {
        fw.Write("export const ");
        fw.Write(reference.NameCamel);
        fw.Write($"List: {reference.NamePascal}[] = [");
        fw.WriteLine();
        foreach (var refValue in reference.Values)
        {
            fw.WriteLine("    {");
            fw.Write("        ");
            fw.Write(string.Join(",\n        ", refValue.Value.Where(p => p.Value != "null").Select(property => $"{property.Key.NameCamel}: {(Config.GetImplementation(property.Key.Domain)?.Type == "string" ? @$"""{(Config.TranslateReferences == true && property.Key == property.Key.Class.DefaultProperty ? refValue.ResourceKey : property.Value)}""" : @$"{property.Value}")}")));
            fw.WriteLine();
            fw.WriteLine("    },");
        }

        fw.WriteLine("];");
        fw.WriteLine();
    }
}