using TopModel.Core;
using TopModel.Generator.Core;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JavaEnumConstructorGenerator(JpaConfig config)
    : JavaConstructorGenerator(config)
{
    public void WriteEnumConstructor(JavaWriter fw, Class classe, IEnumerable<Class> availableClasses, string tag)
    {
        var codeProperty = classe.EnumKey!;
        fw.WriteLine();
        fw.WriteDocStart(1, "Enum constructor");
        fw.WriteParam(classe.EnumKey!.NameCamel, "Code dont on veut obtenir l'instance");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"public {classe.NamePascal}({Config.GetType(classe.EnumKey!)} {classe.EnumKey!.NameCamel}) {{");
        if (classe.Extends != null || classe.Decorators.Any(d => Config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            fw.WriteLine(2, $"super();");
        }

        fw.WriteLine(2, $@"this.{classe.EnumKey!.NameCamel} = {classe.EnumKey!.NameCamel};");
        if (classe.GetProperties(availableClasses).Count > 1)
        {
            fw.WriteLine(2, $@"switch({classe.EnumKey!.NameCamel}) {{");
            foreach (var refValue in classe.Values.OrderBy(x => x.Name, StringComparer.Ordinal))
            {
                var code = refValue.Value[codeProperty];
                fw.WriteLine(2, $@"case {code} :");
                foreach (var prop in classe.GetProperties(availableClasses).Where(p => p != codeProperty))
                {
                    var isString = Config.GetType(prop) == "String";
                    var value = refValue.Value.TryGetValue(prop, out var v) ? v : "null";
                    if (value == "null")
                    {
                        isString = false;
                    }
                    else if (prop is AssociationProperty ap && Config.CanClassUseEnums(ap.Association, prop: ap.Property) && ap.Association.Values.Any(r => r.Value.ContainsKey(ap.Property) && r.Value[ap.Property] == value))
                    {
                        value = ap.Association.NamePascal + "." + value;
                        isString = false;
                        fw.AddImport(ap.Association.GetImport(Config, tag));
                    }
                    else if (prop is AliasProperty alp && Config.CanClassUseEnums(alp.Property.Class, prop: alp.Property))
                    {
                        value = Config.GetType(alp.Property) + "." + value;
                    }
                    else if (Config.TranslateReferences == true && classe.DefaultProperty == prop && !Config.CanClassUseEnums(classe, prop: prop))
                    {
                        value = refValue.ResourceKey;
                    }

                    var quote = isString ? "\"" : string.Empty;
                    var val = quote + value + quote;
                    fw.WriteLine(3, $@"this.{prop.NameByClassCamel} = {val};");
                }

                fw.WriteLine(3, $@"break;");
            }

            fw.WriteLine(2, $@"}}");
        }

        fw.WriteLine(1, $"}}");
    }
}
