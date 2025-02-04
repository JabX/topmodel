using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Generator.Core;

namespace TopModel.Generator.Jpa.ClassGeneration;

/// <summary>
/// Générateur de DAOs JPA.
/// </summary>
public class JpaDaoGenerator : ClassGeneratorBase<JpaConfig>
{
    private readonly ILogger<JpaDaoGenerator> _logger;

    public JpaDaoGenerator(ILogger<JpaDaoGenerator> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public override string Name => "JpaDaoGen";

    protected override bool FilterClass(Class classe)
    {
        return classe.IsPersistent && (!Config.UseJdbc || classe.PrimaryKey.Count() <= 1) && !Config.CanClassUseEnums(classe, Classes);
    }

    protected override string GetFileName(Class classe, string tag)
    {
        string path = Config.DaosAbstract ? $"Abstract{classe.NamePascal}DAO.java" : $"{classe.NamePascal}DAO.java";
        return Path.Combine(
            Config.OutputDirectory,
            Config.ResolveVariables(Config.DaosPath!, tag, module: classe.Namespace.Module).ToFilePath(),
            path);
    }

    protected override void HandleClass(string fileName, Class classe, string tag)
    {
        // Ne génère le DAO qu'une seule fois
        if (!Config.DaosAbstract && File.Exists(fileName))
        {
            return;
        }

        var packageName = Config.ResolveVariables(
            Config.DaosPath!,
            tag,
            module: classe.Namespace.Module).ToPackageName();

        using var fw = new JavaWriter(fileName, _logger, packageName, null);
        fw.WriteLine();
        if (Config.CanClassUseEnums(classe))
        {
            fw.AddImport($"{Config.GetEnumPackageName(classe, tag)}.{Config.GetType(classe.PrimaryKey.SingleOrDefault() ?? classe.Extends!.PrimaryKey.Single())}");
        }

        string pk;
        if (!classe.PrimaryKey.Any() && classe.Extends != null)
        {
            pk = Config.GetType(classe.ExtendedProperties.Single(p => p.PrimaryKey));
            fw.AddImports(classe.ExtendedProperties.Single(p => p.PrimaryKey).GetTypeImports(Config, tag));
        }
        else
        {
            if (classe.PrimaryKey.Count() > 1)
            {
                pk = $"{classe.NamePascal}.{classe.NamePascal}Id";
            }
            else
            {
                pk = Config.GetType(classe.PrimaryKey.Single());
                fw.AddImports(classe.PrimaryKey.Single().GetTypeImports(Config, tag));
            }
        }

        string daosInterface;
        fw.AddImport(classe.GetImport(Config, tag));
        if (Config.DaosInterface != null)
        {
            int lastIndexOf = Config.DaosInterface.LastIndexOf(".");
            string daosInterfaceName = lastIndexOf > -1 ? Config.DaosInterface.Substring(lastIndexOf + 1) : Config.DaosInterface;
            daosInterface = $"{daosInterfaceName}<{classe.NamePascal}, {pk}>";
            fw.AddImport($"{Config.DaosInterface}");
        }
        else if (classe.Reference || Config.UseJdbc)
        {
            daosInterface = $"CrudRepository<{classe.NamePascal}, {pk}>";
            fw.AddImport("org.springframework.data.repository.CrudRepository");
        }
        else
        {
            daosInterface = $"JpaRepository<{classe.NamePascal}, {pk}>";
            fw.AddImport("org.springframework.data.jpa.repository.JpaRepository");
        }

        if (Config.DaosAbstract)
        {
            fw.WriteLine("@NoRepositoryBean");
            fw.AddImport("org.springframework.data.repository.NoRepositoryBean");
            fw.WriteLine($"interface Abstract{classe.NamePascal}DAO extends {daosInterface} {{");
        }
        else
        {
            fw.WriteLine($"public interface {classe.NamePascal}DAO extends {daosInterface} {{");
        }

        fw.WriteLine();
        fw.WriteLine("}");
    }
}