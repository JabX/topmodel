using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.Model.Implementation;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JavaDtoGenerator : JavaClassGeneratorBase
{
    private readonly ILogger<JavaDtoGenerator> _logger;

    public JavaDtoGenerator(ILogger<JavaDtoGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JavaDtoGen";

    private List<Class> AvailableClasses => Classes.ToList();

    protected override bool FilterClass(Class classe)
    {
        return !classe.Abstract && !classe.IsPersistent && !Config.CanClassUseEnums(classe, Classes);
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Path.Combine(
            Config.OutputDirectory,
            Config.ResolveVariables(Config.DtosPath, tag, module: classe.Namespace.Module).ToFilePath(),
            $"{classe.NamePascal}.java");
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        var packageName = Config.GetPackageName(classe, tag);
        using var fw = new JavaWriter(fileName, _logger, packageName, null);

        fw.WriteLine();

        WriteAnnotations(fw, classe, tag);

        var extends = Config.GetClassExtends(classe);
        if (classe.Extends is not null)
        {
            fw.AddImport($"{Config.GetPackageName(classe.Extends, tag)}.{classe.Extends.NamePascal}");
            fw.AddImport(classe.Extends.GetImport(Config, Config.GetBestClassTag(classe.Extends, tag)));
        }

        var implements = Config.GetClassImplements(classe).ToList();

        implements.Add("Serializable");
        fw.AddImport("java.io.Serializable");

        fw.WriteClassDeclaration(classe.NamePascal, null, extends, implements);

        WriteStaticMembers(fw, classe, tag);
        JpaModelPropertyGenerator.WriteProperties(fw, classe, tag);
        WriteConstuctors(fw, classe, tag);

        WriteGetters(fw, classe, tag);
        WriteSetters(fw, classe, tag);
        if (Config.MappersInClass)
        {
            WriteToMappers(fw, classe, tag);
        }

        if ((Config.FieldsEnum & Target.Dto) > 0)
        {
            WriteFieldsEnum(fw, classe, tag);
        }

        fw.WriteLine("}");
    }

    protected virtual void WriteConstuctors(JavaWriter fw, Class classe, string tag)
    {
        if (Config.MappersInClass && classe.FromMappers.Any(c => c.ClassParams.All(p => Classes.Contains(p.Class)))
            || classe.Extends != null
            || Classes.Any(c => c.Extends == classe)
            || classe.Decorators.Any(d => Config.GetImplementation(d.Decorator)?.Extends is not null))
        {
            ConstructorGenerator.WriteNoArgConstructor(fw, classe);
        }

        if (Config.MappersInClass)
        {
            ConstructorGenerator.WriteFromMappers(fw, classe, AvailableClasses, tag);
        }
    }

    protected virtual void WriteStaticMembers(JavaWriter fw, Class classe, string tag)
    {
        fw.WriteLine("	/** Serial ID */");
        fw.WriteLine(1, "private static final long serialVersionUID = 1L;");
    }
}