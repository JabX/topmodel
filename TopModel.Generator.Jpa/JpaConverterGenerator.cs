using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de DAOs JPA.
/// </summary>
public class JpaConverterGenerator : ClassGeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaConverterGenerator> _logger;

    public JpaConverterGenerator(ILogger<JpaConverterGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaConverterGen";

    protected override bool FilterClass(Class classe)
    {
        return classe.IsPersistent && (!Config.UseJdbc || classe.PrimaryKey.Count() <= 1) && Config.CanClassUseEnums(classe) && Config.EnumsAsEnum;
    }

    protected override string GetFileName(Class classe, string tag)
    {
        string path = $"{classe.NamePascal}Converter.java";
        return Path.Combine(
            Config.OutputDirectory,
            Config.ResolveVariables(Config.EnumConverterPath!, tag, module: classe.Namespace.Module).ToFilePath(),
            path);
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        var packageName = Config.ResolveVariables(
            Config.EnumConverterPath!,
            tag,
            module: classe.Namespace.Module).ToPackageName();

        using var fw = new JavaWriter(fileName, _logger, packageName, null);
        fw.WriteLine();
        fw.AddImport(classe.GetImport(Config, tag));
        var javaOrJakarta = Config.PersistenceMode.ToString().ToLower();
        fw.AddImport($"{javaOrJakarta}.persistence.AttributeConverter");
        List<string> implements = new()
        {
            $"AttributeConverter<{classe.NamePascal}, String>"
        };
        fw.WriteClassDeclaration($"{classe.NamePascal}Converter", null, null, implements);

        fw.WriteLine();
        fw.WriteLine(1, "@Override");
        fw.WriteLine(1, $@"public String convertToDatabaseColumn({classe.NamePascal} item) {{");
        fw.WriteLine(2, $"return item == null ? null : item.get{classe.EnumKey.NamePascal}().name();");
        fw.WriteLine(1, "}");
        WriteEnumEntityToAttribute(fw, classe);
        fw.WriteLine("}");
    }

    private void WriteEnumEntityToAttribute(JavaWriter fw, Class classe)
    {
        fw.WriteLine();
        fw.WriteLine(1, "@Override");
        fw.WriteLine(1, $"public {classe.NamePascal} convertToEntityAttribute(String {classe.EnumKey!.NameCamel}) {{");
        fw.WriteLine(2, $@"return {classe.NamePascal}.from({classe.EnumKey!.NameCamel});");

        fw.WriteLine(1, $"}}");
    }
}