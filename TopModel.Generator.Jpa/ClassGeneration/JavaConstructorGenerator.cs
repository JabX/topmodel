using TopModel.Core;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JavaConstructorGenerator
{
    public JavaConstructorGenerator(JpaConfig config)
    {
        Config = config;
    }

    protected JpaConfig Config { get; set; }

    public void WriteFromMappers(JavaWriter fw, Class classe, IEnumerable<Class> availableClasses, string tag)
    {
        var fromMappers = classe.FromMappers.Where(c => c.ClassParams.All(p => availableClasses.Contains(p.Class))).Select(m => (classe, m))
            .OrderBy(m => m.classe.NamePascal)
            .ToList();

        foreach (var fromMapper in fromMappers)
        {
            var (clazz, mapper) = fromMapper;
            fw.AddImport(clazz.GetImport(Config, tag));
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

                fw.AddImport(param.Class.GetImport(Config, tag));
                fw.WriteParam(param.Name.ToCamelCase(), $"Instance de '{param.Class.NamePascal}'");
            }

            foreach (var param in mapper.PropertyParams)
            {
                fw.WriteParam(param.Property.NameCamel, param.Property.Comment);
            }

            fw.WriteReturns(1, $"Une nouvelle instance de '{classe.NamePascal}'");
            fw.WriteDocEnd(1);
            var entryParams = mapper.ClassParams.Select(p => $"{p.Class} {p.Name.ToCamelCase()}").Concat(mapper.PropertyParams.Select(p => $"{Config.GetType(p.Property, availableClasses)} {p.Property.NameCamel}"));
            var entryParamImports = mapper.PropertyParams.Select(p => p.Property.GetTypeImports(Config, tag)).SelectMany(p => p);
            fw.AddImports(entryParamImports.ToList());
            fw.WriteLine(1, $"public {classe.NamePascal}({string.Join(", ", entryParams)}) {{");
            if (classe.Extends != null)
            {
                fw.WriteLine(2, $"super();");
            }

            var (mapperNs, mapperModelPath) = Config.GetMapperLocation(fromMapper);
            fw.WriteLine(2, $"{Config.GetMapperName(mapperNs, mapperModelPath)}.create{classe.NamePascal}({string.Join(", ", mapper.ClassParams.Select(p => p.Name.ToCamelCase()).Concat(mapper.PropertyParams.Select(p => p.Property.NameCamel)))}, this);");
            fw.AddImport(Config.GetMapperImport(mapperNs, mapperModelPath, tag)!);
            fw.WriteLine(1, "}");
        }
    }

    public void WriteNoArgConstructor(JavaWriter fw, Class classe)
    {
        fw.WriteLine();
        var constructor = new JavaConstructor(classe.NamePascal)
        {
            Visibility = "public",
            Comment = "No arg constructor"
        };
        if (classe.Extends != null || classe.Decorators.Any(d => Config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            constructor.AddBodyLine("super();");
        }

        constructor.AddBodyLine("// No arg constructor");
        fw.Write(1, constructor);
    }
}
