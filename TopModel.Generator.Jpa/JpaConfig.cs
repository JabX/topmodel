using System.Text.RegularExpressions;
using TopModel.Core;
using TopModel.Core.FileModel;
using TopModel.Core.Model.Implementation;
using TopModel.Generator.Core;
using TopModel.Utils;

namespace TopModel.Generator.Jpa;

public class JpaConfig : GeneratorConfigBase
{
    /// <summary>
    /// Transforme les classes contenant des values en enum. Par défaut, false.
    /// </summary>
    public bool EnumsAsEnums { get; set; } = false;

    /// <summary>
    /// Localisation des classes persistées du modèle, relative au répertoire de génération. Par défaut, 'javagen/{app}/entities/{module}'.
    /// </summary>
    public string EntitiesPath { get; set; } = "javagen:{app}/entities/{module}";

    /// <summary>
    /// Localisation des enums, relative au répertoire de génération. Par défaut, 'javagen:{app}/enums/{module}'.
    /// </summary>
    public string EnumsPath { get; set; } = "javagen:{app}/enums/{module}";

    /// <summary>
    /// Localisation des DAOs, relative au répertoire de génération.
    /// </summary>
    public string? DaosPath { get; set; }

    /// <summary>
    /// Localisation des classses non persistées du modèle, relative au répertoire de génération. Par défaut, 'javagen/{app}/dtos/{module}'.
    /// </summary>
    public string DtosPath { get; set; } = "javagen:{app}/dtos/{module}";

    /// <summary>
    /// Localisation du l'API générée (client ou serveur), relative au répertoire de génération. Par défaut, 'javagen/{app}/api/{module}'.
    /// </summary>
    public string ApiPath { get; set; } = "javagen:{app}/api/{module}";

    /// <summary>
    /// Mode de génération de l'API ("Client" ou "Server").
    /// </summary>
    public string? ApiGeneration { get; set; }

    /// <summary>
    /// Mode de génération de l'API Client (RestClient, RestTemplate ou FeignClient).
    /// </summary>
    public string? ClientApiGeneration { get; set; } = ClientApiMode.RestClient;

    /// <summary>
    /// Localisation des ressources, relative au répertoire de génération.
    /// </summary>
    public string? ResourcesPath { get; set; }

    /// <summary>
    /// Localisation des ressources, relative au répertoire de génération.
    /// </summary>
    public ResourcesEncoding? ResourcesEncoding { get; set; } = Jpa.ResourcesEncoding.Latin1;

    /// <summary>
    /// Nom du schéma sur lequel les entités sont sauvegardées
    /// </summary>
    public string? DbSchema { get; set; }

    /// <summary>
    /// Nom complet de la classe permettant de convertir les compositions stockées en json dans la bdd
    /// </summary>
    public string CompositionConverterCanonicalName { get; set; } = "{package}.{class}Converter";

    public string CompositionConverterSimpleName => CompositionConverterCanonicalName.Split('.').Last();

    public string JavaxOrJakarta => PersistenceMode.ToString().ToLower();

    public JavaAnnotation GeneratedAnnotation => new JavaAnnotation("Generated", imports: $"{JavaxOrJakarta}.annotation.Generated")
                .AddAttribute("value", "\"TopModel : https://github.com/klee-contrib/topmodel\"");

    public override Dictionary<string, List<string>> TemplateAttributes => new()
    {
        [nameof(CompositionConverterCanonicalName)] = ["package", "class"]
    };

    /// <summary>
    /// Option pour générer des adders pour les associations oneToMany et ManyToMany
    /// </summary>
    public bool AssociationAdders { get; set; } = false;

    /// <summary>
    /// Option pour générer des removers pour les associations oneToMany et ManyToMany
    /// </summary>
    public bool AssociationRemovers { get; set; } = false;

    /// <summary>
    /// Option pour générer l'annotation @Generated("TopModel : https://github.com/klee-contrib/topmodel")
    /// </summary>
    public bool GeneratedHint { get; set; } = true;

    /// <summary>
    /// Option pour générer une enum des champs des classes persistées
    /// </summary>
    public Target FieldsEnum { get; set; } = Target.None;

    /// <summary>
    /// Précise l'interface des fields enum générés.
    /// </summary>
    public string? FieldsEnumInterface { get; set; }

    /// <summary>
    /// Précise le nom du package dans lequel générer les controllers.
    /// </summary>
    public PersistenceMode PersistenceMode { get; set; } = PersistenceMode.Javax;

    /// <summary>
    /// Mode de génération des séquences.
    /// </summary>
    public IdentityConfig Identity { get; set; } = new() { Mode = IdentityMode.IDENTITY };

    /// <summary>
    /// Location des flux de données générés.
    /// </summary>
    public string? DataFlowsPath { get; set; }

    /// <summary>
    /// Génération en mode JDBC.
    /// </summary>
    public bool UseJdbc { get; set; } = false;

    /// <summary>
    /// Génération d'interface Abstract avec @NoRepositoryBean permettant de mettre à jour le code généré.
    /// </summary>
    public bool DaosAbstract { get; set; } = false;

    /// <summary>
    /// Précise l'interface des Daos générés.
    /// </summary>
    public string? DaosInterface { get; set; }

    /// <summary>
    /// Indique s'il faut ajouter les mappers en tant méthode ou constructeur dans les classes qui les déclarent.
    /// </summary>
    public bool MappersInClass { get; set; } = true;

    /// <summary>
    /// Taille des chunks à extraire et insérer
    /// </summary>
    public long DataFlowsBulkSize { get; set; } = 100000;

    /// <summary>
    /// Listeners à ajouter aux dataflows
    /// </summary>
    public List<string> DataFlowsListeners { get; set; } = [];

    public override string[] PropertiesWithLangVariableSupport =>
    [
        nameof(ResourcesPath)
    ];

    public override string[] PropertiesWithTagVariableSupport =>
    [
        nameof(EntitiesPath),
        nameof(DaosPath),
        nameof(DtosPath),
        nameof(ApiPath),
        nameof(EnumsPath),
        nameof(EnumsValuesPath),
        nameof(ApiGeneration),
        nameof(ResourcesPath),
        nameof(DbSchema)
    ];

    public override string[] PropertiesWithModuleVariableSupport =>
    [
        nameof(EntitiesPath),
        nameof(DaosPath),
        nameof(DtosPath),
        nameof(ApiPath),
        nameof(EnumsPath),
        nameof(EnumsValuesPath),
        nameof(DataFlowsPath)
    ];

    /// <summary>
    /// Localisation des enums de valeurs, relative au répertoire de génération. Par défaut, 'javagen:{app}/enums/{module}'.
    /// </summary>
    public string EnumsValuesPath { get; set; } = "default";

    public override bool CanClassUseEnums(Class classe, IEnumerable<Class>? availableClasses = null, IProperty? prop = null)
    {
        return !UseJdbc && base.CanClassUseEnums(classe, availableClasses, prop)
            && !classe.Properties.OfType<AssociationProperty>().Any(a => a.Association != classe && !CanClassUseEnums(a.Association, availableClasses));
    }

    public string GetApiPath(ModelFile file, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(ApiPath!, tag, module: file.Namespace.Module).ToFilePath());
    }

    public string GetBestClassTag(Class classe, string tag)
    {
        return classe.Tags.Contains(tag) ? tag : classe.Tags.Intersect(Tags).FirstOrDefault() ?? tag;
    }

    public string GetClassFileName(Class classe, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(classe.IsPersistent ? EntitiesPath : DtosPath, tag, module: classe.Namespace.Module).ToFilePath(),
            $"{classe.NamePascal}.java");
    }

    public string GetDataFlowConfigFilePath(string module)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(DataFlowsPath!, module: module).ToFilePath()
            .ToFilePath(),
            $"{module.ToPascalCase()}JobConfiguration.java");
    }

    public string GetDataFlowFilePath(DataFlow df, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(DataFlowsPath!, tag: tag, module: df.ModelFile.Namespace.ModulePath).ToFilePath(),
            $"{df.Name.ToPascalCase()}Flow.java");
    }

    public string GetDataFlowPartialFilePath(DataFlow df, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(DataFlowsPath!, tag: tag, module: df.ModelFile.Namespace.ModulePath).ToFilePath(),
            $"{df.Name.ToPascalCase()}PartialFlow.java");
    }

    public IEnumerable<JavaAnnotation> GetDomainJavaAnnotations(IProperty property, string tag)
    {
        return GetDomainAnnotationsAndImports(property, tag).Select(a =>
        {
            return new JavaAnnotation(name: a.Annotation, imports: a.Imports.ToArray());
        });
    }

    public string GetEnumFileName(IProperty property, Class classe, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(EnumsPath, tag, module: classe.Namespace.Module).ToFilePath(),
            $"{GetEnumName(property, classe)}.java");
    }

    public string GetEnumName(IProperty property, Class classe)
    {
        return $"{classe.NamePascal}{property.Name.ToPascalCase()}";
    }

    public string GetEnumPackageName(Class classe, string tag)
    {
        return GetPackageName(classe.Namespace, EnumsPath, tag);
    }

    public string GetEnumValueFileName(Class classe, string tag)
    {
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(EnumsValuesPath, tag, module: classe.Namespace.Module).ToFilePath(),
            $"{classe.NamePascal}.java");
    }

    public string GetEnumValuePackageName(Class classe, string tag)
    {
        return GetPackageName(classe.Namespace, EnumsValuesPath, tag);
    }

    public string GetMapperFilePath((Class Classe, FromMapper Mapper) mapper, string tag)
    {
        var (ns, modelPath) = GetMapperLocation(mapper);
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(modelPath, tag: tag, module: ns.Module).ToFilePath(),
            $"{GetMapperName(ns, modelPath)}.java");
    }

    public string GetMapperFilePath((Class Classe, ClassMappings Mapper) mapper, string tag)
    {
        var (ns, modelPath) = GetMapperLocation(mapper);
        return Path.Combine(
            OutputDirectory,
            ResolveVariables(modelPath, tag: tag, module: ns.Module).ToFilePath(),
            $"{GetMapperName(ns, modelPath)}.java");
    }

    public string GetMapperImport(Namespace ns, string modelPath, string tag)
    {
        return $@"{GetPackageName(ns, modelPath, tag)}.{GetMapperName(ns, modelPath)}";
    }

    public (Namespace Namespace, string ModelPath) GetMapperLocation((Class Classe, FromMapper Mapper) mapper)
    {
        if (mapper.Classe.IsPersistent)
        {
            return (mapper.Classe.Namespace, EntitiesPath);
        }

        var persistentParam = mapper.Mapper.ClassParams.FirstOrDefault(p => p.Class.IsPersistent);
        if (persistentParam != null)
        {
            return (persistentParam.Class.Namespace, EntitiesPath);
        }

        return (mapper.Classe.Namespace, DtosPath);
    }

    public (Namespace Namespace, string ModelPath) GetMapperLocation((Class Classe, ClassMappings Mapper) mapper)
    {
        if (mapper.Classe.IsPersistent)
        {
            return (mapper.Classe.Namespace, EntitiesPath);
        }

        if (mapper.Mapper.Class.IsPersistent)
        {
            return (mapper.Mapper.Class.Namespace, EntitiesPath);
        }

        return (mapper.Classe.Namespace, DtosPath);
    }

    public string GetMapperName(Namespace ns, string modelPath)
    {
        return $"{ns.ModuleFlat}{(modelPath == EntitiesPath ? string.Empty : "DTO")}Mappers".ToPascalCase();
    }

    public string GetPackageName(Endpoint endpoint, string tag)
    {
        return GetPackageName(endpoint.Namespace, ApiPath, tag);
    }

    public string GetPackageName(Class classe, string tag, bool? isPersistent = null)
    {
        return GetPackageName(
            classe.Namespace,
            isPersistent.HasValue ? isPersistent.Value ? EntitiesPath : DtosPath : classe.IsPersistent ? EntitiesPath : DtosPath,
            tag);
    }

    public string GetPackageName(Namespace ns, string modelPath, string tag)
    {
        return ResolveVariables(modelPath, tag, module: ns.Module).ToPackageName();
    }

    public bool IsEnumNameJavaValid(string name)
    {
        return IsEnumNameValid(name);
    }

    protected override string GetConstEnumName(string className, string refName)
    {
        return $"{className.ToPascalCase()}.{refName}";
    }

    protected override string GetEnumType(string className, string propName, bool isPrimaryKeyDef = false)
    {
        if (EnumsAsEnums)
        {
            return $"{className.ToPascalCase()}";
        }

        return $"{className.ToPascalCase()}{propName.ToPascalCase()}";
    }

    protected override bool IsEnumNameValid(string name)
    {
        return base.IsEnumNameValid(name) && !Regex.IsMatch(name ?? string.Empty, "(?<=[^$\\w'\"\\])(?!(abstract|assert|boolean|break|byte|case|catch|char|class|const|continue|default|double|do|else|enum|extends|false|final|finally|float|for|goto|if|implements|import|instanceof|int|interface|long|native|new|null|package|private|protected|public|return|short|static|strictfp|super|switch|synchronized|this|throw|throws|transient|true|try|void|volatile|while|_\\b))([A-Za-z_$][$\\w]*)");
    }

}