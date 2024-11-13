using Microsoft.Extensions.Logging;
using TopModel.Core;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JavaEnumDtoGenerator : JavaDtoGenerator
{
    private JavaEnumConstructorGenerator? _jpaModelConstructorGenerator;

    public JavaEnumDtoGenerator(ILogger<JavaEnumDtoGenerator> logger)
        : base(logger)
    {
    }

    public override string Name => "JpaEnumDtoGen";

    protected override JavaEnumConstructorGenerator ConstructorGenerator
    {
        get
        {
            _jpaModelConstructorGenerator ??= new JavaEnumConstructorGenerator(Config);
            return _jpaModelConstructorGenerator;
        }
    }

    protected override bool FilterClass(Class classe)
    {
        return !classe.Abstract
        && Config.CanClassUseEnums(classe, Classes)
        && !Config.EnumsAsEnums
        && !classe.IsPersistent;
    }

    protected override void WriteConstuctors(JavaWriter fw, Class classe, string tag)
    {
        ConstructorGenerator.WriteNoArgConstructor(fw, classe);
        ConstructorGenerator.WriteEnumCodeFinder(fw, classe, tag);
        ConstructorGenerator.WriteEnumValueConstructor(fw, classe, Classes, tag);
    }

    protected override void WriteSetters(JavaWriter fw, Class classe, string tag)
    {
        return;
    }

    protected override void WriteStaticMembers(JavaWriter fw, Class classe, string tag)
    {
        base.WriteStaticMembers(fw, classe, tag);
        fw.WriteLine();
        var codeProperty = classe.EnumKey!;
        foreach (var refValue in classe.Values.OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            var code = refValue.Value[codeProperty];
            fw.AddImport($"{Config.GetEnumValuePackageName(classe, tag)}.{classe.NamePascal}{Config.EnumValueSuffix}");
            fw.WriteLine(1, $@"public static final {classe.NamePascal} {code} = new {classe.NamePascal}({classe.NamePascal}{Config.EnumValueSuffix}.{code});");
        }
    }
}
