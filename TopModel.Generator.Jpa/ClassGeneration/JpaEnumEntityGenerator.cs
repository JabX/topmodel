using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Utils;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JpaEnumEntityGenerator(ILogger<JpaEnumEntityGenerator> logger, GeneratedFileWriterProvider writerProvider)
    : JpaEntityGenerator(logger, writerProvider)
{
    private JavaEnumConstructorGenerator? _javaEnumConstructorGenerator;

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
        return !classe.Abstract && Config.CanClassUseEnums(classe, Classes) && classe.IsPersistent;
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        var packageName = Config.GetPackageName(classe, tag);
        using var fw = this.OpenJavaWriter(fileName, packageName, null);

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
            var code = refValue.Value[codeProperty];
            if (classe.IsPersistent)
            {
                fw.AddImport($"{JavaxOrJakarta}.persistence.Transient");
                fw.WriteLine(1, "@Transient");
            }

            fw.WriteLine(1, $@"public static final {classe.NamePascal} {code} = new {classe.NamePascal}({Config.GetEnumName(codeProperty, classe)}.{code});");
        }

        JpaModelPropertyGenerator.WriteProperties(fw, classe, tag);
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
        ConstructorGenerator.WriteEnumConstructor(fw, classe, Classes, tag);
    }

    protected override void WriteSetters(JavaWriter fw, Class classe, string tag)
    {
        return;
    }
}