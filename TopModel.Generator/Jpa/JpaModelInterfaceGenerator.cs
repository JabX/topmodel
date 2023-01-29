﻿using Microsoft.Extensions.Logging;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

/// <summary>
/// Générateur de DAOs JPA.
/// </summary>
public class JpaModelInterfaceGenerator : GeneratorBase<object>
{
    private readonly JpaConfig _config;
    private readonly ILogger<JpaModelInterfaceGenerator> _logger;

    public JpaModelInterfaceGenerator(ILogger<JpaModelInterfaceGenerator> logger, JpaConfig config)
        : base(logger, config)
    {
        _config = config;
        _logger = logger;
    }

    public override string Name => "JpaInterfaceGen";

    public override IEnumerable<string> GeneratedFiles => Files
        .SelectMany(f => f.Value.Classes)
        .Where(c => c.Decorators
            .Any(d => d.Decorator.Java != null && d.Decorator.Java.GenerateInterface))
        .Select(c => GetFileClassName(c));

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
        var packageRoot = classe.IsPersistent ? _config.EntitiesPackageName : _config.DtosPackageName;
        return Path.Combine(
                _config.OutputDirectory,
                _config.ModelRootPath,
                Path.Combine(packageRoot.Split(".")),
                classe.Namespace.Module.Replace('.', Path.DirectorySeparatorChar).ToLower(),
                "interfaces");
    }

    private string GetClassName(Class classe)
    {
        return $"I{classe.Name}";
    }

    private string GetFileClassName(Class classe)
    {
        return Path.Combine(GetDestinationFolder(classe), $"{GetClassName(classe)}.java");
    }

    private void GenerateModule(string module)
    {
        var classes = Classes
            .Where(c => c.Decorators.Any(d => d.Decorator.Java != null && d.Decorator.Java.GenerateInterface))
            .Where(c => c.Namespace.Module == module);
        foreach (var classe in classes)
        {
            var packageRoot = classe.IsPersistent ? _config.EntitiesPackageName : _config.DtosPackageName;
            var destFolder = GetDestinationFolder(classe);
            var dirInfo = Directory.CreateDirectory(destFolder);
            var packageName = $"{packageRoot}.{classe.Namespace.Module.ToLower()}.interfaces";
            using var fw = new JavaWriter(GetFileClassName(classe), _logger, packageName, null);

            WriteImports(fw, classe);
            fw.WriteLine();

            var extendsDecorator = classe.Decorators.SingleOrDefault(d => d.Decorator.Java?.Extends != null);
            var extends = (classe.Extends?.Name ?? extendsDecorator.Decorator?.Java!.Extends!.ParseTemplate(classe, extendsDecorator.Parameters)) ?? null;

            var implements = classe.Decorators.SelectMany(d => d.Decorator.Java!.Implements.Select(i => i.ParseTemplate(classe, d.Parameters))).Distinct().ToList();

            fw.WriteLine("@Generated(\"TopModel : https://github.com/klee-contrib/topmodel\")");
            fw.WriteLine($"public interface I{classe.Name} {{");

            WriteGetters(fw, classe);

            fw.WriteLine("}");
        }
    }

    private void WriteGetters(JavaWriter fw, Class classe)
    {
        foreach (var property in classe.Properties.Where(p => !_config.EnumShortcutMode || !(p is AssociationProperty apo && apo.Association.Reference && (apo.Type == AssociationType.OneToOne || apo.Type == AssociationType.ManyToOne))))
        {
            var getterPrefix = property.GetJavaType().ToUpper() == "BOOLEAN" ? "is" : "get";
            fw.WriteLine();
            fw.WriteDocStart(1, $"Getter for {property.GetJavaName()}");
            fw.WriteReturns(1, $"value of {{@link {classe.GetImport(_config)}#{property.GetJavaName()} {property.GetJavaName()}}}");
            fw.WriteDocEnd(1);
            fw.WriteLine(1, @$"{property.GetJavaType()} {getterPrefix}{property.GetJavaName().ToFirstUpper()}();");
        }
    }

    private void WriteImports(JavaWriter fw, Class classe)
    {
        var imports = new List<string>
            {
                _config.PersistenceMode.ToString().ToLower() + ".annotation.Generated",
            };
        foreach (var property in classe.Properties)
        {
            imports.AddRange(property.GetTypeImports(_config));

            if (property is CompositionProperty cp && cp.Composition.Namespace.Module == cp.Class?.Namespace.Module)
            {
                imports.Add(cp.Composition.GetImport(_config));
            }
        }

        if (classe.Extends != null)
        {
            foreach (var property in classe.Extends.Properties)
            {
                imports.AddRange(property.GetTypeImports(_config));
            }
        }

        fw.AddImports(imports);
    }
}