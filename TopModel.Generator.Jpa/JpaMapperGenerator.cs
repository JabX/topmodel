using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

public class JpaMapperGenerator : MapperGeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaMapperGenerator> _logger;

    private JpaModelPropertyGenerator? _jpaModelPropertyGenerator;

    public JpaMapperGenerator(ILogger<JpaMapperGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaMapperGenerator";

    protected JpaModelPropertyGenerator JpaModelPropertyGenerator
    {
        get
        {
            _jpaModelPropertyGenerator ??= Config.UseJdbc ? new JdbcModelPropertyGenerator(Config, Classes, []) : new JpaModelPropertyGenerator(Config, Classes, []);
            return _jpaModelPropertyGenerator;
        }
    }

    protected override string GetFileName((Class Classe, FromMapper Mapper) mapper, string tag)
    {
        return Config.GetMapperFilePath(mapper, GetBestClassTag(mapper.Classe, tag));
    }

    protected override string GetFileName((Class Classe, ClassMappings Mapper) mapper, string tag)
    {
        return Config.GetMapperFilePath(mapper, GetBestClassTag(mapper.Classe, tag));
    }

    protected override void HandleFile(string fileName, string tag, IList<(Class Classe, FromMapper Mapper)> fromMappers, IList<(Class Classe, ClassMappings Mapper)> toMappers)
    {
        var sampleFromMapper = fromMappers.FirstOrDefault();
        var sampleToMapper = toMappers.FirstOrDefault();

        var (mapperNs, modelPath) = sampleFromMapper != default
            ? Config.GetMapperLocation(sampleFromMapper)
            : Config.GetMapperLocation(sampleToMapper);

        var package = Config.GetPackageName(mapperNs, modelPath, GetBestClassTag(sampleFromMapper.Classe != null ? sampleFromMapper.Classe : sampleToMapper.Classe, tag));

        using var fw = new JavaWriter(fileName, _logger, package, null);

        var imports = fromMappers.SelectMany(m => m.Mapper.ClassParams.Select(p => p.Class).Concat([m.Classe]))
            .Concat(toMappers.SelectMany(m => new[] { m.Classe, m.Mapper.Class }))
            .Where(c => Classes.Contains(c))
            .Select(c => c.GetImport(Config, c.Tags.Contains(tag) ? tag : c.Tags.Intersect(Config.Tags).First()))
            .Distinct()
            .ToArray();
        var associationImports = fromMappers.SelectMany(m => m.Mapper.ClassParams.Select(p => p.Class).Concat([m.Classe]))
            .Concat(toMappers.SelectMany(m => new[] { m.Classe, m.Mapper.Class }))
            .SelectMany(c => c.Properties.OfType<AssociationProperty>())
            .Where(ap => Config.CanClassUseEnums(ap.Association) && Config.EnumsAsEnums)
            .Select(ap => $"{Config.GetEnumValuePackageName(ap.Association, Config.GetBestClassTag(ap.Association, tag))}.{ap.Association.NamePascal}{Config.EnumValueSuffix}")
            .ToArray();
        fw.AddImports(imports);
        fw.AddImports(associationImports);
        fw.WriteLine();
        if (Config.GeneratedHint)
        {
            fw.WriteLine(0, Config.GeneratedAnnotation);
        }

        fw.WriteLine($@"public class {Config.GetMapperName(mapperNs, modelPath)} {{");

        fw.WriteLine();
        fw.WriteLine(1, $@"private {Config.GetMapperName(mapperNs, modelPath)}() {{");
        fw.WriteLine(2, "// private constructor to hide implicite public one");
        fw.WriteLine(1, "}");

        foreach (var (classe1, mapper) in fromMappers)
        {
            WriteFromMapper(classe1, mapper, fw, GetBestClassTag(classe1, tag));
        }

        foreach (var (classe, mapper1) in toMappers)
        {
            WriteToMapper(classe, mapper1, fw, GetBestClassTag(classe, tag));
        }

        fw.WriteLine("}");
    }

    private (string Getter, bool CheckSourceNull) GetSourceGetter(IProperty propertySource, IProperty propertyTarget, Class classe, JavaWriter fw, string sourceName, string tag)
    {
        var getter = string.Empty;
        var getterName = JpaModelPropertyGenerator.GetGetterName(propertySource);
        var converter = Config.GetConverter(propertySource.Domain, propertyTarget.Domain);
        if (converter != null && Config.GetImplementation(converter) != null)
        {
            var impl = Config.GetImplementation(converter);
            if (impl != null)
            {
                fw.AddImports(impl.Imports);
            }
        }

        if (Config.UseJdbc)
        {
            getter = $"{sourceName}.{getterName}()";
            return (Getter: Config.GetConvertedValue(
                getter,
                propertySource.Domain,
                propertyTarget.Domain), CheckSourceNull: false);
        }

        var checkSourceNull = false;
        if (
            (!propertySource.Class.IsPersistent && !propertyTarget.Class.IsPersistent)
             || !(propertySource is AssociationProperty
                || propertySource is AliasProperty psAlp && psAlp.Property is AssociationProperty
                || propertyTarget is AssociationProperty
                || propertySource is AliasProperty ptAlp && ptAlp.Property is AssociationProperty))
        {
            getter = $"{sourceName}.{getterName}()";
        }
        else if (propertySource.Class.IsPersistent && (!propertyTarget.Class.IsPersistent || !(propertyTarget is AssociationProperty)) && (propertySource is AssociationProperty apSource && apSource.Association.IsPersistent || propertySource is AliasProperty alpSource && alpSource.Property is AssociationProperty apSource2 && apSource2.Association.IsPersistent))
        {
            apSource = propertySource is AssociationProperty ? (AssociationProperty)propertySource : (AssociationProperty)((AliasProperty)propertySource).Property;
            checkSourceNull = true;
            if (propertyTarget is CompositionProperty cp)
            {
                if (propertySource.Class.ToMappers.Any(t => t.Class == cp.Composition))
                {
                    var cpMapper = propertySource.Class.ToMappers.Find(t => t.Class == cp.Composition)!;
                    var (cpMapperNs, cpMapperModelPath) = Config.GetMapperLocation((cpMapper.Class, cpMapper));

                    getter = $"{Config.GetMapperName(cpMapperNs, cpMapperModelPath)}.{cpMapper.Name.Value.ToCamelCase()}({sourceName}.{getterName}(), target.get{apSource.NameByClassPascal}())";
                    fw.AddImport(Config.GetMapperImport(cpMapperNs, cpMapperModelPath, tag)!);
                }
                else if (cp.Composition.FromMappers.Any(f => f.Params.Count == 1 && f.ClassParams.First().Class == apSource.Association))
                {
                    var cpMapper = cp.Composition.FromMappers.Find(f => f.Params.Count == 1 && f.ClassParams.First().Class == apSource.Association)!;
                    var (cpMapperNs, cpMapperModelPath) = Config.GetMapperLocation((cp.Composition, cpMapper));

                    getter = $"{sourceName}.{getterName}()";
                    if (apSource.Type.IsToMany())
                    {
                        getter = $"{getter}.stream().map(item -> {Config.GetMapperName(cpMapperNs, cpMapperModelPath)}.create{cp.Composition}(item, null)).collect(Collectors.toList())";
                        fw.AddImport("java.util.stream.Collectors");
                    }
                    else
                    {
                        getter = $"{Config.GetMapperName(cpMapperNs, cpMapperModelPath)}.create{cp.Composition}({getter}, target.get{propertyTarget.NameByClassPascal}())";
                    }

                    fw.AddImport(Config.GetMapperImport(cpMapperNs, cpMapperModelPath, tag)!);
                }
                else
                {
                    throw new ModelException(classe, $"La propriété {propertySource.Name} ne peut pas être mappée avec la propriété {propertyTarget.Name} car il n'existe pas de mapper {cp.Composition.Name} -> {apSource.Association.Name}");
                }
            }
            else
            {
                if (apSource.Type == AssociationType.OneToOne || apSource.Type == AssociationType.ManyToOne)
                {
                    getter = $"{sourceName}.{getterName}().get{apSource.Property.NameByClassPascal}()";
                }
                else
                {
                    getter = $"{sourceName}.{getterName}().stream().filter(Objects::nonNull).map({apSource.Association.NamePascal}::get{apSource.Property.NameByClassPascal}).collect(Collectors.toList())";
                    fw.AddImport("java.util.stream.Collectors");
                    fw.AddImport("java.util.Objects");
                    fw.AddImport(apSource.Association.GetImport(Config, tag));
                }
            }
        }
        else if ((!propertySource.Class.IsPersistent || !(propertySource is AssociationProperty)) && propertyTarget.Class.IsPersistent && (propertyTarget is AssociationProperty apTarget && apTarget.Association.IsPersistent || propertyTarget is AliasProperty ptAp && ptAp.Property is AssociationProperty ptApAss && ptApAss.Association.IsPersistent))
        {
            apTarget = propertyTarget is AssociationProperty ? (AssociationProperty)propertyTarget : (AssociationProperty)((AliasProperty)propertyTarget).Property;
            if (Config.CanClassUseEnums(apTarget.Property.Class))
            {
                if (!propertySource.Class.IsPersistent)
                {
                    if (apTarget.Type.IsToMany())
                    {
                        getter = $@"{sourceName}.{getterName}().stream().map({apTarget.Association.NamePascal}::from).collect(Collectors.toList())";
                        fw.AddImport("java.util.stream.Collectors");
                    }
                    else
                    {
                        getter = $"{apTarget.Association.NamePascal}.from({sourceName}.{getterName}())";
                        fw.AddImport(apTarget.Association.GetImport(Config, tag));
                        checkSourceNull = true;
                    }
                }
                else
                {
                    getter = $@"{sourceName}.{getterName}()";
                    if (!apTarget.Type.IsToMany())
                    {
                        checkSourceNull = true;
                    }
                }
            }
            else if (propertyTarget.Class.IsPersistent && propertySource.Class.IsPersistent)
            {
                getter = $"{sourceName}.{getterName}()";
            }
            else if (propertySource is CompositionProperty cp)
            {
                if (cp.Composition.ToMappers.Any(t => t.Class == apTarget.Association))
                {
                    var cpMapper = cp.Composition.ToMappers.Find(t => t.Class == apTarget.Association)!;
                    var (cpMapperNs, cpMapperModelPath) = Config.GetMapperLocation((cpMapper.Class, cpMapper));

                    var isMultiple = apTarget.Type == AssociationType.OneToMany || apTarget.Type == AssociationType.ManyToMany;
                    if (isMultiple)
                    {
                        getter = $@"{sourceName}.{getterName}(){(!propertySource.Class.IsPersistent ? $".stream().map(src -> {Config.GetMapperName(cpMapperNs, cpMapperModelPath)}.{cpMapper.Name.ToCamelCase()}(src, null)).collect(Collectors.toList())" : string.Empty)}";
                        fw.AddImport("java.util.stream.Collectors");
                    }
                    else
                    {
                        getter = $"{Config.GetMapperName(cpMapperNs, cpMapperModelPath)}.{cpMapper.Name.Value.ToCamelCase()}({sourceName}.{getterName}(), target.get{apTarget.NameByClassPascal}())";
                        checkSourceNull = true;
                        fw.AddImport(Config.GetMapperImport(cpMapperNs, cpMapperModelPath, tag)!);
                    }
                }
                else
                {
                    throw new ModelException(classe, $"La propriété {propertySource.Name} ne peut pas être mappée avec la propriété {propertyTarget.Name} car il n'existe pas de mapper {cp.Composition.Name} -> {apTarget.Association.Name}");
                }
            }
        }
        else
        {
            getter = $"{sourceName}.{getterName}()";
        }

        return (Getter: Config.GetConvertedValue(
                getter,
                propertySource.Domain,
                propertyTarget.Domain), CheckSourceNull: checkSourceNull);
    }

    private void WriteFromMapper(Class classe, FromMapper mapper, JavaWriter fw, string tag)
    {
        if (Config.CanClassUseEnums(classe, Classes))
        {
            _logger.LogWarning($"La classe {classe.Name} ne peut pas être mappée car c'est une enum");
            return;
        }

        fw.WriteLine();
        fw.WriteDocStart(1, $"Map les champs des classes passées en paramètre dans l'objet target'");
        fw.WriteParam("target", $"Instance de '{classe}' (ou null pour créer une nouvelle instance)");
        foreach (var param in mapper.ClassParams)
        {
            if (param.Comment != null)
            {
                fw.WriteParam(param.Name.ToCamelCase(), param.Comment);
            }
            else
            {
                fw.WriteParam(param.Name.ToCamelCase(), $"Instance de '{param.Class}'");
            }
        }

        foreach (var param in mapper.PropertyParams)
        {
            fw.WriteParam(param.Property.NameCamel, param.Property.Comment);
        }

        fw.WriteReturns(1, $"Une nouvelle instance de '{classe.NamePascal}' ou bien l'instance passée en paramètres sur lesquels les champs sources ont été mappée");
        fw.WriteDocEnd(1);
        var useClassForAssociation = (IProperty p) => classe.IsPersistent && !Config.UseJdbc && p is AssociationProperty ap && ap.Association.IsPersistent;
        var entryParams = mapper.ClassParams.Select(p => $"{p.Class} {p.Name.ToCamelCase()}").Concat(mapper.PropertyParams.Select(p => $"{Config.GetType(p.Property, Classes, useClassForAssociation: useClassForAssociation(p.Property))} {p.Property.NameCamel}"));
        var entryParamImports = mapper.PropertyParams.Select(p => p.Property.GetTypeImports(Config, tag)).SelectMany(p => p);
        fw.AddImports(entryParamImports.ToList());
        fw.WriteLine(1, $"public static {classe.NamePascal} create{classe.NamePascal}({string.Join(", ", entryParams)}, {classe.NamePascal} target) {{");
        fw.WriteLine(2, "if (target == null) {");
        if (classe.Abstract)
        {
            fw.WriteLine(3, $"throw new IllegalArgumentException(\"target cannot be null\");");
        }
        else
        {
            fw.WriteLine(3, $"target = new {classe.NamePascal}();");
        }

        fw.WriteLine(2, "}");
        fw.WriteLine();
        var hydrate = string.Empty;
        if (classe.Abstract)
        {
            hydrate = "target.hydrate(";
        }

        var isFirst = true;

        foreach (var param in mapper.ClassParams.Where(p => p.Mappings.Count > 0))
        {
            if (param.Required && !classe.Abstract)
            {
                fw.WriteLine(2, $"if ({param.Name.ToCamelCase()} == null) {{");
                fw.WriteLine(3, $"throw new IllegalArgumentException(\"{param.Name} cannot be null\");");
                fw.WriteLine(2, "}");
                fw.WriteLine();
            }
        }

        foreach (var param in mapper.PropertyParams)
        {
            if (param.Property.Required && !classe.Abstract)
            {
                if (param.TargetProperty is AssociationProperty atg && atg.Association.IsPersistent && classe.IsPersistent)
                {
                    continue;
                }

                fw.WriteLine(2, $"if ({param.Property.NameCamel} == null) {{");
                fw.WriteLine(3, $"throw new IllegalArgumentException(\"{param.Property.NameCamel} cannot be null\");");
                fw.WriteLine(2, "}");
                fw.WriteLine();
            }
        }

        foreach (var param in mapper.ClassParams.Where(p => p.Mappings.Count > 0))
        {
            var mappings = param.Mappings.ToList();
            var indent = 2;
            if (!param.Required)
            {
                fw.WriteLine(indent, $"if ({param.Name.ToCamelCase()} != null) {{");
                indent++;
            }

            foreach (var mapping in mappings)
            {
                var propertyTarget = mapping.Key;
                var propertySource = mapping.Value!;
                var getterPrefix = Config.GetType(propertyTarget!) == "boolean" ? "is" : "get";
                var (getter, checkSourceNull) = GetSourceGetter(propertySource, propertyTarget, classe, fw, param.Name.ToCamelCase(), tag);
                var propertyTargetName = Config.UseJdbc || propertyTarget is AssociationProperty ap && !ap.Association.IsPersistent ? propertyTarget.NamePascal : propertyTarget.NameByClassPascal;
                if (classe.Abstract)
                {
                    if (!isFirst)
                    {
                        hydrate += ", ";
                    }
                    else
                    {
                        isFirst = false;
                    }

                    if (checkSourceNull)
                    {
                        hydrate += $"{param.Name}.{propertyTargetName.WithPrefix(getterPrefix)}() != null ? {getter} : null";
                    }
                    else
                    {
                        hydrate += getter;
                    }
                }
                else
                {
                    if (getter != string.Empty)
                    {
                        if (checkSourceNull)
                        {
                            fw.WriteLine(indent, $"if ({param.Name}.{propertySource.NameByClassPascal.WithPrefix(getterPrefix)}() != null) {{");
                        }

                        fw.WriteLine(indent + (checkSourceNull ? 1 : 0), $"target.{propertyTargetName!.WithPrefix("set")}({getter});");

                        if (checkSourceNull)
                        {
                            fw.WriteLine(indent, $"}}");
                            fw.WriteLine();
                        }
                    }
                }
            }

            if (!param.Required)
            {
                fw.WriteLine(indent - 1, "}");
                fw.WriteLine();
            }
        }

        foreach (var param in mapper.PropertyParams)
        {
            var propertyTargetName = Config.UseJdbc ? param.TargetProperty.NamePascal : param.TargetProperty.NameByClassPascal;
            if (param.TargetProperty is AssociationProperty apTg && apTg.Association.IsPersistent && classe.IsPersistent)
            {
                continue;
            }

            if (classe.Abstract)
            {
                if (!isFirst)
                {
                    hydrate += ", ";
                }
                else
                {
                    isFirst = false;
                }

                hydrate += param.Property.NameCamel;
            }
            else
            {
                fw.WriteLine(2, $"target.{propertyTargetName.WithPrefix("set")}({param.Property.NameCamel});");
            }
        }

        if (classe.Abstract)
        {
            hydrate += ");";
            fw.WriteLine(2, hydrate);
        }

        fw.WriteLine(2, "return target;");
        fw.WriteLine(1, "}");
    }

    private void WriteToMapper(Class classe, ClassMappings mapper, JavaWriter fw, string tag)
    {
        if (Config.CanClassUseEnums(mapper.Class, Classes))
        {
            _logger.LogWarning($"La classe {mapper.Class.Name} ne peut pas être mappée car c'est une enum");
            return;
        }

        fw.WriteLine();
        fw.WriteDocStart(1, $"Mappe '{classe}' vers '{mapper.Class.NamePascal}'");
        if (mapper.Comment != null)
        {
            fw.WriteLine(1, $" * {mapper.Comment}");
        }

        fw.WriteParam("source", $"Instance de '{classe}'");
        fw.WriteParam("target", $"Instance pré-existante de '{mapper.Class.NamePascal}'. Une nouvelle instance sera créée si non spécifié.");

        fw.WriteReturns(1, $"Une nouvelle instance de '{mapper.Class.NamePascal}' ou bien l'instance passée en paramètre dont les champs ont été surchargés");
        fw.WriteDocEnd(1);

        fw.WriteLine(1, $"public static {mapper.Class.NamePascal} {mapper.Name.Value.ToCamelCase()}({classe} source, {mapper.Class.NamePascal} target) {{");
        fw.WriteLine(2, "if (source == null) {");
        fw.WriteLine(3, $"throw new IllegalArgumentException(\"source cannot be null\");");
        fw.WriteLine(2, "}");
        fw.WriteLine();
        fw.WriteLine(2, "if (target == null) {");
        if (mapper.Class.Abstract)
        {
            fw.WriteLine(3, $"throw new IllegalArgumentException(\"target cannot be null\");");
        }
        else
        {
            fw.WriteLine(3, $"target = new {mapper.Class.NamePascal}();");
        }

        fw.WriteLine(2, "}");
        fw.WriteLine();
        var hydrate = string.Empty;
        if (mapper.Class.Abstract)
        {
            hydrate = "target.hydrate(";
        }

        var isFirst = true;
        foreach (var mapping in mapper.Mappings.OrderBy(m => m.Key.Class.Properties.IndexOf(m.Key)))
        {
            var propertyTarget = mapping.Value;
            var propertySource = mapping.Key;
            var getterPrefix = Config.GetType(propertyTarget!) == "boolean" ? "is" : "get";
            var (getter, checkSourceNull) = GetSourceGetter(propertySource, propertyTarget!, classe, fw, "source", tag);
            var propertyTargetName = Config.UseJdbc || propertyTarget is AssociationProperty asp && !asp.Association.IsPersistent ? propertyTarget!.NamePascal : propertyTarget!.NameByClassPascal;
            if (mapper.Class.Abstract)
            {
                if (!isFirst)
                {
                    hydrate += ", ";
                }
                else
                {
                    isFirst = false;
                }

                if (checkSourceNull)
                {
                    hydrate += $"source.{propertyTargetName.WithPrefix(getterPrefix)}() != null ? {getter} : null";
                }
                else
                {
                    hydrate += getter;
                }
            }
            else
            {
                if (getter != string.Empty)
                {
                    if (checkSourceNull)
                    {
                        fw.WriteLine(2, $"if (source.{propertySource.NameByClassPascal.WithPrefix(getterPrefix)}() != null) {{");
                    }

                    fw.WriteLine(2 + (checkSourceNull ? 1 : 0), $"target.{propertyTargetName.WithPrefix("set")}({getter});");

                    if (checkSourceNull)
                    {
                        fw.WriteLine(2, $"}}");
                        fw.WriteLine();
                    }
                }
            }
        }

        if (mapper.Class.Abstract)
        {
            hydrate += ");";
            fw.WriteLine(2, hydrate);
        }

        fw.WriteLine(2, "return target;");
        fw.WriteLine(1, "}");
    }
}