using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaEnumGenerator : GeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaEnumGenerator> _logger;

    public JpaEnumGenerator(ILogger<JpaEnumGenerator> logger, ModelConfig modelConfig)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaEnumGen";

    public override IEnumerable<string> GeneratedFiles => Files.Values.SelectMany(f => f.Classes.Where(FilterClass))
        .SelectMany(c => Config.Tags.Intersect(c.Tags).SelectMany(tag => GetEnumProperties(c).Select(p => GetFileName(p, c, tag)))).Distinct();

    protected bool FilterClass(Class classe)
    {
        return !classe.Abstract && Config.CanClassUseEnums(classe, Classes.ToList());
    }

    protected string GetFileName(IProperty property, Class classe, string tag)
    {
        return Config.GetEnumFileName(property, classe, tag);
    }

    protected void HandleClass(Class classe, string tag)
    {
        foreach (var p in GetEnumProperties(classe))
        {
            WriteEnum(p, classe, tag);
        }
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

    private IEnumerable<IProperty> GetEnumProperties(Class classe)
    {
        List<IProperty> result = new();
        if (classe.EnumKey != null && Config.CanClassUseEnums(classe, prop: classe.EnumKey) && !(classe.Extends != null && Config.CanClassUseEnums(classe.Extends, Classes, prop: classe.EnumKey)))
        {
            result.Add(classe.EnumKey);
        }

        var uks = classe.UniqueKeys.Where(uk => uk.Count == 1 && Config.CanClassUseEnums(classe, Classes, uk.Single()) && !(classe.Extends != null && Config.CanClassUseEnums(classe.Extends, Classes, prop: classe.EnumKey))).Select(uk => uk.Single());
        result.AddRange(uks);
        return result;
    }

    private void WriteEnum(IProperty property, Class classe, string tag)
    {
        var packageName = Config.GetEnumPackageName(classe, tag);
        using var fw = new JavaWriter(Config.GetEnumFileName(property, classe, tag), _logger, packageName, null);
        fw.WriteLine();
        var codeProperty = classe.EnumKey!;
        fw.WriteDocStart(0, $"Enumération des valeurs possibles de la propriété {codeProperty.NamePascal} de la classe {classe.NamePascal}");
        fw.WriteDocEnd(0);
        fw.WriteLine($@"public enum {Config.GetEnumName(property, classe)} {{");
        var i = 0;

        var refs = GetAllValues(classe)
            .ToList();

        var properties = classe.Properties.Where(p => p != codeProperty);
        foreach (var refValue in refs)
        {
            i++;
            var isLast = i == refs.Count();
            if (classe.DefaultProperty != null)
            {
                fw.WriteDocStart(1, $"{refValue.Value[classe.DefaultProperty]}");
                fw.WriteDocEnd(1);
            }

            List<string> enumAsString = new List<string> { };
            enumAsString.Add($"{refValue.Value[property]}(");
            foreach (var prop in properties)
            {
                var isString = Config.GetType(prop) == "String";
                var isInt = Config.GetType(prop) == "int";
                var isBoolean = Config.GetType(prop) == "Boolean";
                var value = refValue.Value.ContainsKey(prop) ? refValue.Value[prop] : "null";

                if (prop is AssociationProperty ap && codeProperty.PrimaryKey && ap.Association.Values.Any(r => r.Value.ContainsKey(ap.Property) && r.Value[ap.Property] == value))
                {
                    fw.AddImport($"{Config.GetEnumPackageName(ap.Association.EnumKey.Class, tag)}.{ap.Association.NamePascal + ap.Association.EnumKey}");
                    value = ap.Association.NamePascal + ap.Association.EnumKey + "." + value;
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
                enumAsString.Add($@"{val}{(prop == properties.Last() ? string.Empty : ", ")}");
            }

            enumAsString.Add($"){(isLast ? ";" : ",")} ");
            fw.WriteLine(1, enumAsString.Aggregate(string.Empty, (acc, curr) => acc + curr));
        }

        foreach (var prop in properties)
        {
            fw.WriteLine();
            fw.WriteDocStart(1, $@"{prop.NameByClassPascal}");
            fw.WriteDocEnd(1);
            fw.WriteLine(1, $@"private final {Config.GetType(prop)} {prop.NameByClassCamel};");
        }

        WriteConstructor(property, classe, fw, properties);

        foreach (var prop in properties)
        {
            fw.WriteLine();
            fw.WriteDocStart(1, "Getter");
            fw.WriteDocEnd(1);
            fw.WriteLine(1, $@"public {Config.GetType(prop)} get{prop.NameByClassCamel.ToFirstUpper()}(){{");
            fw.WriteLine(2, $@"return this.{prop.NameByClassCamel};");
            fw.WriteLine(1, $@"}}");
        }

        fw.WriteLine("}");
    }

    private void WriteConstructor(IProperty property, Class classe, JavaWriter fw, IEnumerable<IProperty> properties)
    {
        // Constructeur
        fw.WriteDocStart(1, "Enum constructor");
        fw.WriteDocEnd(1);
        List<string> constructorAsString = new List<string>();
        constructorAsString.Add($@"{Config.GetEnumName(property, classe)}(");

        constructorAsString.Add(properties.Select((prop, index) =>
           $@"final {Config.GetType(prop)} {prop.NameByClassCamel} {(prop == properties.Last() ? string.Empty : ",")}")
           .Aggregate(string.Empty, (acc, curr) => acc + curr));

        constructorAsString.Add("){");
        fw.WriteLine(1, constructorAsString.Aggregate(string.Empty, (acc, curr) => acc + curr));
        foreach (var prop in properties)
        {
            // Constructeur set
            fw.WriteLine(2, $@" this.{prop.NameByClassCamel} = {prop.NameByClassCamel};");
        }

        fw.WriteLine(1, "}");
    }
}