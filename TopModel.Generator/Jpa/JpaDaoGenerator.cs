﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de DAOs JPA.
/// </summary>
public class JpaDaoGenerator : GeneratorBase<object>
{
    private readonly JpaConfig _config;
    private readonly ILogger<JpaDaoGenerator> _logger;

    public JpaDaoGenerator(ILogger<JpaDaoGenerator> logger, JpaConfig config)
        : base(logger, config)
    {
        _config = config;
        _logger = logger;
    }

    public override string Name => "JpaDaoGen";

    public override IEnumerable<string> GeneratedFiles => Files.SelectMany(f => f.Value.Classes).Select(c => GetFileClassName(c));

    protected override void HandleFiles(IEnumerable<ModelFile> files)
    {
        var modules = files.SelectMany(f => f.Classes.Select(c => c.Namespace.Module)).Distinct();

        foreach (var module in modules)
        {
            GenerateModule(module);
        }
    }

    private string GetDestinationFolder(Class classe)
    {
        return Path.Combine(_config.OutputDirectory, _config.ModelRootPath, Path.Combine(_config.DaosPackageName.Split(".")), classe.Namespace.Module.Replace('.', Path.DirectorySeparatorChar).ToLower());
    }

    private string GetFileClassName(Class classe)
    {
        return Path.Combine(GetDestinationFolder(classe), $"{classe.Name}DAO.java");
    }

    private void GenerateModule(string module)
    {
        var classes = Classes.Where(c => c.Namespace.Module == module);

        foreach (var classe in classes.Where(c => c.IsPersistent))
        {
            var destFolder = GetDestinationFolder(classe);
            var dirInfo = Directory.CreateDirectory(destFolder);
            var packageName = $"{_config.DaosPackageName}.{classe.Namespace.Module.ToLower()}";
            var fileName = GetFileClassName(classe);

            var fileExists = File.Exists(fileName);

            // Ne génère le DAO qu'une seule fois
            if (fileExists)
            {
                continue;
            }

            using var fw = new JavaWriter(fileName, _logger, packageName, null);
            fw.WriteLine();
            WriteImports(fw, classe);
            fw.WriteLine();
            fw.WriteLine($"public interface {classe.Name}DAO extends {(classe.Reference ? "CrudRepository" : "JpaRepository")}<{classe.Name}, {classe.PrimaryKey!.GetJavaType()}> {{");
            fw.WriteLine();
            fw.WriteLine("}");
        }
    }

    private void WriteImports(JavaWriter fw, Class classe)
    {
        var imports = new List<string>
            {
                $"{_config.EntitiesPackageName}.{classe.Namespace.Module.ToLower()}.{classe.Name}"
            };
        if (classe.Reference)
        {
            imports.Add(
            "org.springframework.data.repository.CrudRepository");
        }
        else
        {
            imports.Add(
            "org.springframework.data.jpa.repository.JpaRepository");
        }

        fw.AddImports(imports);
    }
}