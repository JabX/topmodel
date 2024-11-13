using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaEnumEntityGenerator : JpaEntityGenerator
{
    private readonly ILogger<JpaEnumEntityGenerator> _logger;

    private JavaEnumConstructorGenerator? _javaEnumConstructorGenerator;

    public JpaEnumEntityGenerator(ILogger<JpaEnumEntityGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaEnumEntityGen";

    protected override JavaEnumConstructorGenerator ConstructorGenerator
    {
        get
        {
            _javaEnumConstructorGenerator ??= new JavaEnumConstructorGenerator(Config);
            return _javaEnumConstructorGenerator;
        }
    }

    protected override bool FilterClass(Class classe)
    {
        return !classe.Abstract
        && Config.CanClassUseEnums(classe, Classes)
        && classe.IsPersistent
        && !Config.EnumsAsEnums;
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
        }

        var implements = Config.GetClassImplements(classe).ToList();

        fw.WriteClassDeclaration(classe.NamePascal, null, extends, implements);
        fw.WriteLine();

        var codeProperty = classe.EnumKey!;
        foreach (var refValue in classe.Values.OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            var code = refValue.Name.ToConstantCase();
            fw.AddImport($"{JavaxOrJakarta}.persistence.Transient");
            fw.WriteLine(1, "@Transient");

            fw.AddImport($"{Config.GetEnumValuePackageName(classe, tag)}.{classe.NamePascal}{Config.EnumValueSuffix}");
            fw.WriteLine(1, $@"public static final {classe.NamePascal} {code} = new {classe.NamePascal}({classe.NamePascal}{Config.EnumValueSuffix}.{code});");
        }

        JpaModelPropertyGenerator.WriteProperties(fw, classe, tag, classe.Properties);
        WriteConstructors(classe, tag, fw);

        WriteGetters(fw, classe, tag);

        if (Config.MappersInClass)
        {
            WriteToMappers(fw, classe, tag);
        }

        if ((Config.FieldsEnum & Target.Persisted) > 0)
        {
            WriteFieldsEnum(fw, classe, tag);
        }

        fw.WriteLine("}");
    }

    protected override void WriteConstructors(Class classe, string tag, JavaWriter fw)
    {
        ConstructorGenerator.WriteNoArgConstructor(fw, classe);
        ConstructorGenerator.WriteEnumCodeFinder(fw, classe, tag);
        ConstructorGenerator.WriteEnumValueConstructor(fw, classe, Classes, tag);
    }

    protected override void WriteGetters(JavaWriter fw, Class classe, string tag)
    {
        foreach (var property in classe.Properties)
        {
            JpaModelPropertyGenerator.WriteGetter(fw, tag, property);
        }
    }

    protected override void WriteSetters(JavaWriter fw, Class classe, string tag)
    {
        return;
    }
}