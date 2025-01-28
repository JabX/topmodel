using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de DAOs JPA.
/// </summary>
public class JpaModelInterfaceGenerator : ClassGeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaModelInterfaceGenerator> _logger;

    public JpaModelInterfaceGenerator(ILogger<JpaModelInterfaceGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaInterfaceGen";

    protected override bool FilterClass(Class classe)
    {
        return classe.Abstract;
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Config.GetClassFileName(classe, tag);
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        var packageName = Config.GetPackageName(classe, tag);
        using var fw = new JavaWriter(fileName, _logger, packageName, null);
        var javaxOrJakarta = Config.PersistenceMode.ToString().ToLower();

        WriteImports(fw, classe, tag);
        fw.WriteLine();

        var extends = Config.GetClassExtends(classe);
        var implements = Config.GetClassImplements(classe);

        if (Config.GeneratedHint)
        {
            fw.WriteLine(0, Config.GeneratedAnnotation);
        }

        fw.WriteLine($"public interface {classe.NamePascal} {{");

        WriteGetters(fw, classe, tag);

        if (classe.Properties.Any(p => !p.Readonly))
        {
            WriteHydrate(fw, classe);
        }

        fw.WriteLine("}");
    }

    protected void WriteGetters(JavaWriter fw, Class classe, string tag)
    {
        foreach (var property in classe.Properties.Where(p => !(p is AssociationProperty apo && apo.Association.Reference && (apo.Type == AssociationType.OneToOne || apo.Type == AssociationType.ManyToOne))))
        {
            var getterPrefix = Config.GetType(property) == "boolean" ? "is" : "get";
            fw.WriteLine();
            fw.WriteDocStart(1, $"Getter for {property.NameByClassCamel}");
            fw.WriteReturns(1, $"value of {{@link {classe.GetImport(Config, tag)}#{property.NameByClassCamel} {property.NameByClassCamel}}}");
            fw.WriteDocEnd(1);
            fw.WriteLine(1, @$"{Config.GetType(property)} {property.NameByClassPascal.WithPrefix(getterPrefix)}();");
        }
    }

    protected void WriteHydrate(JavaWriter fw, Class classe)
    {
        var properties = classe.Properties
            .Where(p => !p.Readonly)
            .Where(p => !(p is AssociationProperty apo && apo.Association.Reference && (apo.Type == AssociationType.OneToOne || apo.Type == AssociationType.ManyToOne)));

        if (!properties.Any())
        {
            return;
        }

        fw.WriteLine();
        fw.WriteDocStart(1, $"hydrate values of instance");
        foreach (var property in properties)
        {
            var propertyName = property.NameByClassCamel;
            fw.WriteLine(1, $" * @param {propertyName} value to set");
        }

        fw.WriteDocEnd(1);
        var signature = string.Join(", ", properties.Select(property =>
            {
                var propertyName = property.NameByClassCamel;
                return $@"{Config.GetType(property)} {property.NameByClassCamel}";
            }));

        fw.WriteLine(1, $"void hydrate({signature});");
    }

    protected void WriteImports(JavaWriter fw, Class classe, string tag)
    {
        var imports = new List<string>
            {
                Config.PersistenceMode.ToString().ToLower() + ".annotation.Generated",
            };
        foreach (var property in classe.Properties)
        {
            imports.AddRange(property.GetTypeImports(Config, tag));

            if (property is CompositionProperty cp && cp.Composition.Namespace.Module == cp.Class?.Namespace.Module)
            {
                imports.Add(cp.Composition.GetImport(Config, tag));
            }
        }

        if (classe.Extends != null)
        {
            foreach (var property in classe.Extends.Properties)
            {
                imports.AddRange(property.GetTypeImports(Config, tag));
            }
        }

        fw.AddImports(imports);
    }
}