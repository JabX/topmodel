using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaEnumValuesGenerator : GeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaEnumValuesGenerator> _logger;

    public JpaEnumValuesGenerator(ILogger<JpaEnumValuesGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaEnumValuesGen";

    public override IEnumerable<string> GeneratedFiles => Files
        .Values
        .SelectMany(f => f.Classes.Where(FilterClass))
        .SelectMany(c => Config.Tags.Intersect(c.Tags).SelectMany(tag => GetEnumProperties(c).Select(p => GetFileName(c, tag)))).Distinct();

    protected bool FilterClass(Class classe)
    {
        return !classe.Abstract
            && (Config.CanClassUseEnums(classe, Classes.ToList()) || Config.EnumsAsEnums && classe.Enum)
            && classe.Enum;
    }

    protected string GetFileName(Class classe, string tag)
    {
        return Config.GetEnumValueFileName(classe, tag);
    }

    protected void HandleClass(Class classe, string tag)
    {
        WriteEnum(classe, tag);
    }

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        foreach (var file in files)
        {
            foreach (var classe in file.Classes.Where(FilterClass))
            {
                foreach (var tag in Config.Tags.Intersect(classe.Tags))
                {
                    HandleClass(classe, tag);
                }
            }
        }
    }

    private List<IProperty> GetEnumProperties(Class classe)
    {
        List<IProperty> result = [];
        if (classe.EnumKey != null && Config.CanClassUseEnums(classe, prop: classe.EnumKey) && !(classe.Extends != null && Config.CanClassUseEnums(classe.Extends, Classes, prop: classe.EnumKey)))
        {
            result.Add(classe.EnumKey);
        }

        var uks = classe.UniqueKeys.Where(uk => uk.Count == 1 && Config.CanClassUseEnums(classe, Classes, uk.Single()) && !(classe.Extends != null && Config.CanClassUseEnums(classe.Extends, Classes, prop: classe.EnumKey))).Select(uk => uk.Single());
        result.AddRange(uks);
        return result;
    }

    private void WriteConstructor(Class classe, JavaWriter fw)
    {
        // Constructeur
        fw.WriteDocStart(1, "Enum values constructor");
        fw.WriteDocEnd(1);
        var properties = classe.Properties.Where(p => p != classe.EnumKey);
        var constructor = new JavaConstructor(classe.NamePascal) { Visibility = "private" };
        var methodParams = properties.Select((prop, index) =>
            {
                var fieldName = prop.NameByClassCamel;
                var fieldType = Config.GetType(prop);
                if (prop is AssociationProperty ap)
                {
                    fieldName = $"{ap.NameByClassCamel}";
                    fieldType = $"{ap.Association.NamePascal}";
                }

                return new JavaMethodParameter(fieldType, fieldName) { Final = true };
            });
        constructor.AddParameters(methodParams);
        fw.Write(1, constructor);
    }

    private void WriteEnum(Class classe, string tag)
    {
        var packageName = Config.GetEnumValuePackageName(classe, tag);
        using var fw = new JavaWriter(Config.GetEnumValueFileName(classe, tag), _logger, packageName, null);
        fw.WriteLine();
        var codeProperty = classe.EnumKey!;
        fw.WriteDocStart(0, $"Enumération des valeurs possibles de la classe {classe.NamePascal}");
        fw.WriteDocEnd(0);
        fw.WriteLine($@"public enum {classe.NamePascal} {{");
        var i = 0;

        var refs = GetAllValues(classe)
            .ToList();

        foreach (var refValue in refs)
        {
            if (i > 0)
            {
                fw.WriteLine();
            }

            i++;
            var isLast = i == refs.Count;
            if (classe.DefaultProperty != null)
            {
                fw.WriteDocStart(1, $"{refValue.Value[classe.DefaultProperty]}");
                fw.WriteDocEnd(1);
            }

            List<string> enumAsString = [$"{refValue.Value[classe.EnumKey!].ToConstantCase()}("];
            foreach (var prop in classe.Properties.Where(p => p != classe.EnumKey))
            {
                var isString = Config.GetType(prop) == "String";
                var isInt = Config.GetType(prop) == "int";
                var isBoolean = Config.GetType(prop) == "Boolean";
                var value = refValue.Value.TryGetValue(prop, out var v) ? v : "null";

                if (prop is AssociationProperty ap && ap.Association.Values.Any(r => r.Value.ContainsKey(ap.Property) && r.Value[ap.Property] == value))
                {
                    fw.AddImport($"{Config.GetEnumValuePackageName(ap.Association.EnumKey!.Class, tag)}.{ap.Association.NamePascal}");
                    value = ap.Association.NamePascal + "." + value;
                    isString = false;
                }
                else if (Config.CanClassUseEnums(classe, prop: prop))
                {
                    value = Config.GetType(prop) + "." + value;
                }

                if (Config.TranslateReferences == true && classe.DefaultProperty == prop && !Config.CanClassUseEnums(classe, prop: prop))
                {
                    value = refValue.ResourceKey;
                }

                var quote = isString ? "\"" : string.Empty;
                var val = quote + value + quote;
                enumAsString.Add($@"{val}{(prop == classe.Properties.Last() ? string.Empty : ", ")}");
            }

            enumAsString.Add($"){(isLast ? ";" : ",")} ");
            fw.WriteLine(1, enumAsString.Aggregate(string.Empty, (acc, curr) => acc + curr));
        }

        foreach (var prop in classe.Properties.Where(p => p != classe.EnumKey))
        {
            fw.WriteLine();
            fw.WriteDocStart(1, $@"{prop.NameByClassPascal}");
            fw.WriteDocEnd(1);
            var fieldName = prop.NameByClassCamel;
            if (prop is AssociationProperty ap)
            {
                fieldName = $"{ap.NameByClassCamel}";
                fw.WriteLine(1, $@"private final {ap.Association.NamePascal} {fieldName};");
            }
            else
            {
                fw.WriteLine(1, $@"private final {Config.GetType(prop)} {fieldName};");
            }
        }

        fw.WriteLine();
        WriteConstructor(classe, fw);

        foreach (var prop in classe.Properties.Where(p => p != classe.EnumKey))
        {
            fw.WriteLine();
            var fieldName = prop.NameByClassCamel;
            fw.WriteDocStart(1, $"Getter for {fieldName}");
            fw.WriteDocEnd(1);
            var fieldType = Config.GetType(prop);
            if (prop is AssociationProperty ap && Config.CanClassUseEnums(ap.Association, Classes))
            {
                fieldName = $"{ap.NameByClassCamel}";
                fieldType = $"{ap.Association.NamePascal}";
            }

            var method = new JavaMethod(fieldType, $"get{fieldName.ToFirstUpper()}") { Visibility = "public" };
            method.AddBodyLine($@"return this.{fieldName};");
            fw.Write(1, method);
        }

        fw.WriteLine();
        fw.WriteLine("}");
    }
}