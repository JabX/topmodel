using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaModelConstructorGenerator
{
    private readonly JpaConfig _config;

    public JpaModelConstructorGenerator(JpaConfig config)
    {
        _config = config;
    }

    public void WriteEnumConstructor(JavaWriter fw, Class classe, List<Class> availableClasses, string tag, ModelConfig modelConfig)
    {
        var codeProperty = classe.EnumKey!;
        fw.WriteLine();
        fw.WriteDocStart(1, "Enum constructor");
        fw.WriteParam(classe.EnumKey!.NameCamel, "Code dont on veut obtenir l'instance");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"public {classe.NamePascal}({_config.GetType(classe.EnumKey!)} {classe.EnumKey!.NameCamel}) {{");
        if (classe.Extends != null || classe.Decorators.Any(d => _config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            fw.WriteLine(2, $"super();");
        }

        fw.WriteLine(2, $@"this.{classe.EnumKey!.NameCamel} = {classe.EnumKey!.NameCamel};");
        if (classe.GetProperties(availableClasses).Count > 1)
        {
            foreach (var prop in classe.GetProperties(availableClasses).Where(p => p != codeProperty))
            {
                string val;
                var getterPrefix = _config.GetType(prop!) == "boolean" ? "is" : "get";
                if (prop is AssociationProperty ap && codeProperty.PrimaryKey && ap.Association.Values.Any(r => r.Value.ContainsKey(ap.Property)))
                {
                    var javaType = _config.GetType(prop, useClassForAssociation: classe.IsPersistent && !_config.UseJdbc && prop is AssociationProperty asp && asp.Association.IsPersistent);
                    javaType = javaType.Split("<")[0];
                    val = $@"{classe.EnumKey!.NameCamel}.{prop.NameByClassCamel.WithPrefix(getterPrefix)}() != null ? new {javaType}({classe.EnumKey!.NameCamel}.{prop.NameByClassCamel.WithPrefix(getterPrefix)}()) : null";
                }
                else if (_config.CanClassUseEnums(classe, prop: prop))
                {
                    var javaType = _config.GetType(prop, useClassForAssociation: classe.IsPersistent && !_config.UseJdbc && prop is AssociationProperty asp && asp.Association.IsPersistent);
                    javaType = javaType.Split("<")[0];
                    val = $@"{classe.EnumKey!.NameCamel}.{prop.NameByClassCamel.WithPrefix(getterPrefix)}() != null ? new {javaType}({classe.EnumKey!.NameCamel}.{prop.NameByClassCamel.WithPrefix(getterPrefix)}()) : null";
                }
                else if (_config.TranslateReferences == true && classe.DefaultProperty == prop && !_config.CanClassUseEnums(classe, prop: prop))
                {
                    val = $@"{classe.EnumKey!.NameCamel}.{prop.NameByClassCamel.WithPrefix(getterPrefix)}()";
                }
                else
                {
                    val = $@"{classe.EnumKey!.NameCamel}.{prop.NameByClassCamel.WithPrefix(getterPrefix)}()";
                }

                fw.WriteLine(2, $@"this.{prop.NameByClassCamel} = {val};");

            }
        }

        fw.WriteLine(1, $"}}");
    }

    public void WriteFromMappers(JavaWriter fw, Class classe, List<Class> availableClasses, string tag)
    {
        var fromMappers = classe.FromMappers.Where(c => c.ClassParams.All(p => availableClasses.Contains(p.Class))).Select(m => (classe, m))
            .OrderBy(m => m.classe.NamePascal)
            .ToList();

        foreach (var fromMapper in fromMappers)
        {
            var (clazz, mapper) = fromMapper;
            fw.WriteLine();
            fw.WriteDocStart(1, $"Crée une nouvelle instance de '{classe.NamePascal}'");
            if (mapper.Comment != null)
            {
                fw.WriteLine(1, $" * {mapper.Comment}");
            }

            foreach (var param in mapper.ClassParams)
            {
                if (param.Comment != null)
                {
                    fw.WriteLine(1, $" * {param.Comment}");
                }

                fw.WriteParam(param.Name.ToCamelCase(), $"Instance de '{param.Class.NamePascal}'");
            }

            foreach (var param in mapper.PropertyParams)
            {
                fw.WriteParam(param.Property.NameCamel, param.Property.Comment);
            }

            fw.WriteReturns(1, $"Une nouvelle instance de '{classe.NamePascal}'");
            fw.WriteDocEnd(1);
            var entryParams = mapper.ClassParams.Select(p => $"{p.Class} {p.Name.ToCamelCase()}").Concat(mapper.PropertyParams.Select(p => $"{_config.GetType(p.Property, availableClasses)} {p.Property.NameCamel}"));
            var entryParamImports = mapper.PropertyParams.Select(p => p.Property.GetTypeImports(_config, tag)).SelectMany(p => p);
            fw.AddImports(entryParamImports.ToList());
            fw.WriteLine(1, $"public {classe.NamePascal}({string.Join(", ", entryParams)}) {{");
            if (classe.Extends != null)
            {
                fw.WriteLine(2, $"super();");
            }

            var (mapperNs, mapperModelPath) = _config.GetMapperLocation(fromMapper);
            fw.WriteLine(2, $"{_config.GetMapperName(mapperNs, mapperModelPath)}.create{classe.NamePascal}({string.Join(", ", mapper.ClassParams.Select(p => p.Name.ToCamelCase()).Concat(mapper.PropertyParams.Select(p => p.Property.NameCamel)))}, this);");
            fw.AddImport(_config.GetMapperImport(mapperNs, mapperModelPath, tag)!);
            fw.WriteLine(1, "}");
        }
    }

    public void WriteNoArgConstructor(JavaWriter fw, Class classe)
    {
        fw.WriteLine();
        fw.WriteDocStart(1, "No arg constructor");
        fw.WriteDocEnd(1);
        fw.WriteLine(1, $"public {classe.NamePascal}() {{");
        if (classe.Extends != null || classe.Decorators.Any(d => _config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            fw.WriteLine(2, $"super();");
        }

        fw.WriteLine(2, "// No arg constructor");
        fw.WriteLine(1, $"}}");
    }
}
