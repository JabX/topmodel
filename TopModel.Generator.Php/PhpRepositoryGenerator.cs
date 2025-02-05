using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Php;

/// <summary>
/// Générateur de Repository Php.
/// </summary>
public class PhpRepositoryGenerator(ILogger<PhpRepositoryGenerator> logger, IFileWriterProvider writerProvider)
    : ClassGeneratorBase<PhpConfig>(logger, writerProvider)
{
    public override string Name => "PhpRepositoryGen";

    protected override bool FilterClass(Class classe)
    {
        return classe.IsPersistent;
    }

    protected override string GetFileName(Class classe, string tag)
    {
        return Path.Combine(
            Config.OutputDirectory,
            Config.ResolveVariables(Config.RepositoriesPath, tag, module: classe.Namespace.Module).ToFilePath(),
            $"{classe.NamePascal}Repository.php");
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        // Ne génère le repository qu'une seule fois
        if (File.Exists(fileName))
        {
            return;
        }

        var nameSpace = Config.ResolveVariables(
            Config.RepositoriesPath,
            tag,
            module: classe.Namespace.Module).ToPackageName();

        using var fw = this.OpenPhpWriter(fileName, nameSpace, null);
        fw.WriteDocStart(0, $"@extends ServiceEntityRepository<{classe.NamePascal}>");
        fw.WriteDocEnd(0);
        fw.AddImport(@"Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository");
        fw.WriteLine($"class {classe.NamePascal}Repository extends ServiceEntityRepository");
        fw.WriteLine("{");
        fw.AddImport(@"Symfony\Bridge\Doctrine\ManagerRegistry");
        fw.WriteLine(1, "public function __construct(ManagerRegistry $registry)");
        fw.WriteLine(1, "{");
        fw.AddImport(classe.GetImport(Config, tag));
        fw.WriteLine(2, $"parent::__construct($registry, {classe.NamePascal}::class);");
        fw.WriteLine(1, "}");
        fw.WriteLine("}");
    }
}