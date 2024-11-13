using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JavaEnumConstructorGenerator : JavaConstructorGenerator
{
    public JavaEnumConstructorGenerator(JpaConfig config)
    : base(config)
    {
    }

    public void WriteEnumCodeFinder(JavaWriter fw, Class classe, string tag)
    {
        var codeProperty = classe.EnumKey!;
        fw.WriteLine();
        fw.WriteDocStart(1, "Enum code finder");
        fw.WriteParam(classe.EnumKey!.NameCamel, "Code dont on veut obtenir l'instance");
        fw.WriteDocEnd(1);
        var method = new JavaMethod(classe.NamePascal, "from")
        {
            Static = true,
            Visibility = "public",
        };
        var param = new JavaMethodParameter(Config.GetType(classe.EnumKey!), codeProperty.Name.ToCamelCase());
        method.AddParameter(param);

        method.AddBodyLine($@"return switch ({param.Name}) {{");
        foreach (var refValue in classe.Values.OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            var code = refValue.Name.ToConstantCase();
            method.AddBodyLine(1, $@"case {code} -> {code};");
        }

        method.AddBodyLine("};");

        fw.Write(1, method);
    }

    public void WriteEnumValueConstructor(JavaWriter fw, Class classe, IEnumerable<Class> availableClasses, string tag)
    {
        var codeProperty = classe.EnumKey!;
        fw.WriteLine();
        fw.WriteDocStart(1, "Enum constructor");
        fw.WriteParam($"{classe.NameCamel}{Config.EnumValueSuffix}", "Enum de valeur dont on veut obtenir l'entité");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"public {classe.NamePascal}({classe.NamePascal}{Config.EnumValueSuffix} {classe.NameCamel}{Config.EnumValueSuffix}) {{");
        if (classe.Extends != null || classe.Decorators.Any(d => Config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            fw.WriteLine(2, $"super();");
        }

        foreach (var prop in classe.Properties)
        {
            var value = $"{classe.NameCamel}{Config.EnumValueSuffix}.get{prop.NameByClassPascal}()";
            if (prop is AssociationProperty ap && Config.CanClassUseEnums(ap.Association, prop: ap.Property))
            {
                value = $"new {ap.Association.NamePascal}({classe.NameCamel}{Config.EnumValueSuffix}.get{prop.NameByClassPascal}{Config.EnumValueSuffix}())";
                fw.AddImport(ap.Association.GetImport(Config, tag));
                fw.AddImport($"{Config.GetEnumValuePackageName(ap.Association, tag)}.{ap.Association.NamePascal}{Config.EnumValueSuffix}");
                fw.WriteLine(2, $@"if ({classe.NameCamel}{Config.EnumValueSuffix}.get{prop.NameByClassPascal}{Config.EnumValueSuffix}() != null) {{");
                fw.WriteLine(3, $@"this.{prop.NameByClassCamel} = {value};");
                fw.WriteLine(2, "}");
            }
            else
            {
                fw.WriteLine(2, $@"this.{prop.NameByClassCamel} = {value};");
            }
        }

        fw.WriteLine(1, $"}}");
    }
}
