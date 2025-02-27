﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de fichiers de modèles JPA.
/// </summary>
public class JavaEnumDtoGenerator(ILogger<JavaEnumDtoGenerator> logger, IFileWriterProvider writerProvider)
    : JavaDtoGenerator(logger, writerProvider)
{
    private JavaEnumConstructorGenerator? _jpaModelConstructorGenerator;

    public override string Name => "JavaEnumDtoGen";

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
        return !classe.Abstract && Config.CanClassUseEnums(classe, Classes) && !classe.IsPersistent;
    }

    protected override void WriteConstuctors(JavaWriter fw, Class classe, string tag)
    {
        ConstructorGenerator.WriteNoArgConstructor(fw, classe);
        ConstructorGenerator.WriteEnumConstructor(fw, classe, Classes, tag);
    }

    protected override void WriteSetters(JavaWriter fw, Class classe, string tag)
    {
        return;
    }

    protected override void WriteStaticMembers(JavaWriter fw, Class classe)
    {
        base.WriteStaticMembers(fw, classe);
        fw.WriteLine();
        var codeProperty = classe.EnumKey!;
        foreach (var refValue in classe.Values.OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            var code = refValue.Value[codeProperty];
            fw.WriteLine(1, $@"public static final {classe.NamePascal} {code} = new {classe.NamePascal}({Config.GetEnumName(codeProperty, classe)}.{code});");
        }
    }
}