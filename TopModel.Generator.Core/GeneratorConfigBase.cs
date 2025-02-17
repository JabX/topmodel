using System.Text.RegularExpressions;
using Spectre.Console;
using TopModel.Core;
using TopModel.Core.Model.Implementation;
using TopModel.Utils;

namespace TopModel.Generator.Core;

public abstract class GeneratorConfigBase
{
    /// <summary>
    /// Racine du répertoire de génération.
    /// </summary>
    public required string OutputDirectory { get; set; }

    /// <summary>
    /// Tags du générateur.
    /// </summary>
    public required IList<string> Tags { get; set; }

    /// <summary>
    /// Tags pour lesquels il ne faut pas générer les fichiers (surchage en CLI).
    /// </summary>
    public IList<string> ExcludedTags { get; set; } = [];

    /// <summary>
    /// Désactive la génération des valeurs par défaut des propriétés dans les classes et endpoints générés avec cette configuration.
    /// </summary>
    public virtual bool IgnoreDefaultValues { get; set; }

    /// <summary>
    /// Langage du générateur, utilisé pour choisir l'implémentation correspondante des domaines, décorateurs et convertisseurs.
    /// </summary>
    public required string Language { get; set; }
#nullable enable

    /// <summary>
    /// Si les libellés des listes de références doivent être traduits.
    /// </summary>
    public bool? TranslateReferences { get; set; }

    /// <summary>
    /// Si les libellés des propriétés doivent être traduits.
    /// </summary>
    public bool? TranslateProperties { get; set; }

    /// <summary>
    /// Générateurs désactivés.
    /// </summary>
    public IList<string>? Disable { get; set; }

    /// <summary>
    /// Variables globales du générateur.
    /// </summary>
    public Dictionary<string, string> Variables { get; set; } = [];

    /// <summary>
    /// Variables par tag du générateur.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> TagVariables { get; set; } = [];

    public IEnumerable<string> TagVariableNames => TagVariables.Values.SelectMany(v => v.Keys).Distinct();

    public IEnumerable<string> GlobalVariableNames => Variables.Select(v => v.Key).Except(TagVariableNames).Distinct();

    /// <summary>
    /// Propriétés qui supportent la variable "module".
    /// </summary>
    public virtual string[] PropertiesWithModuleVariableSupport => [];

    /// <summary>
    /// Propriétés qui supportent la variable "module".
    /// </summary>
    public virtual string[] PropertiesWithFileNameVariableSupport => [];

    /// <summary>
    /// Propriétés qui supportent la variable "lang".
    /// </summary>
    public virtual string[] PropertiesWithLangVariableSupport => [];

    /// <summary>
    /// Propriétés qui peuvent contenir des templates à ne pas interprêter dans la résolution des variables globales ou par tag.
    /// </summary>
    public virtual Dictionary<string, List<string>> TemplateAttributes => [];

    /// <summary>
    /// Propriétés qui supportent les variables par tag de la configuration courante.
    /// </summary>
    public virtual string[] PropertiesWithTagVariableSupport => [];

    protected virtual bool UseNamedEnums => true;

    protected virtual string NullValue => "null";

    /// <summary>
    /// Détermine si une classe peut utiliser une enum pour sa clé primaire.
    /// </summary>
    /// <param name="classe">Classe.</param>
    /// <param name="availableClasses">Classes disponibles.</param>
    /// <param name="prop">Propriété à vérifier (si c'est pas la clé primaire).</param>
    /// <returns>Oui/non.</returns>
    public virtual bool CanClassUseEnums(Class classe, IEnumerable<Class>? availableClasses = null, IProperty? prop = null)
    {
        if (availableClasses != null && !availableClasses.Contains(classe))
        {
            return false;
        }

        prop ??= classe.EnumKey;

        bool CheckProperty(IProperty fp)
        {
            return (fp == classe.EnumKey || classe.UniqueKeys.Where(uk => uk.Count == 1).Select(uk => uk.Single()).Contains(prop))
                && classe.Values.All(r => r.Value.ContainsKey(fp) && IsEnumNameValid(r.Value[fp].ToString()));
        }

        return classe.Enum && CheckProperty(prop!);
    }

    public IEnumerable<(string Annotation, List<string> Imports)> GetAnnotationsAndImports(Class classe, string tag)
    {
        return classe.Decorators
            .Where(d => GetImplementation(d.Decorator) != null)
             .SelectMany(d =>
             {
                 var decorator = d.Decorator;
                 var imple = GetImplementation(decorator)!;
                 return imple.Annotations.Select(a => (Annotation: a.ParseTemplate(classe, d.Parameters, this, tag),
                 Imports: imple.Imports.Select(i => i.ParseTemplate(classe, d.Parameters, this, tag)).ToList()));
             });
    }

    public string? GetClassExtends(Class item)
    {
        var extendsDecorator = item.Decorators.SingleOrDefault(d => GetImplementation(d.Decorator)?.Extends != null);
        return item.Extends?.NamePascal ?? GetImplementation(extendsDecorator.Decorator)?.Extends!.ParseTemplate(item, extendsDecorator.Parameters, this);
    }

    public IEnumerable<string> GetClassImplements(Class classe)
    {
        return classe.Decorators.SelectMany(d => (GetImplementation(d.Decorator)?.Implements ?? [])
            .Select(i => i.ParseTemplate(classe, d.Parameters, this)))
            .Distinct();
    }

    public string GetConvertedValue(string value, Domain? fromDomain, Domain? toDomain)
    {
        var converter = GetConverter(fromDomain, toDomain);
        if (converter != null && fromDomain != null && toDomain != null)
        {
            var text = GetImplementation(converter)?.Text;
            if (text != null)
            {
                value = GetImplementation(converter)!.Text
                    .Replace("{value}", value)
                    .ParseTemplate(fromDomain, toDomain, this);
            }
        }

        return value;
    }

    public Converter? GetConverter(Domain? fromDomain, Domain? toDomain)
    {
        if (fromDomain != null && toDomain != null && fromDomain != toDomain)
        {
            return fromDomain.ConvertersFrom.FirstOrDefault(c => c.From.Contains(fromDomain) && c.To.Contains(toDomain));
        }

        return null;
    }

    public IEnumerable<string> GetConverterImports(Domain? fromDomain, Domain? toDomain)
    {
        if (fromDomain != null && toDomain != null && fromDomain != toDomain)
        {
            var converter = GetConverter(fromDomain, toDomain);
            if (converter != null)
            {
                var imports = GetImplementation(converter)?.Imports;
                if (imports != null)
                {
                    foreach (var import in imports)
                    {
                        yield return import;
                    }
                }
            }
        }
    }

    public IEnumerable<string> GetDecoratorAnnotations(Class classe, string tag)
    {
        return classe.Decorators.SelectMany(d => (GetImplementation(d.Decorator)?.Annotations ?? [])
            .Select(a => a.ParseTemplate(classe, d.Parameters, this, tag)))
            .Distinct();
    }

    public IEnumerable<string> GetDecoratorAnnotations(Endpoint endpoint, string tag)
    {
        return endpoint.Decorators.SelectMany(d => (GetImplementation(d.Decorator)?.Annotations ?? [])
            .Select(a => a.ParseTemplate(endpoint, d.Parameters, this, tag)))
            .Distinct();
    }

    public IEnumerable<string> GetDecoratorImports(Class classe, string tag)
    {
        return classe.Decorators.SelectMany(d => (GetImplementation(d.Decorator)?.Imports ?? [])
            .Select(i => i.ParseTemplate(classe, d.Parameters, this, tag)))
            .Distinct();
    }

    public IEnumerable<string> GetDecoratorImports(Endpoint endpoint, string tag)
    {
        return endpoint.Decorators.SelectMany(d => (GetImplementation(d.Decorator)?.Imports ?? [])
            .Select(i => i.ParseTemplate(endpoint, d.Parameters, this, tag)))
            .Distinct();
    }

    public IEnumerable<string> GetDomainAnnotations(IProperty property, string tag)
    {
        return GetDomainAnnotationsAndImports(property, tag).Select(a => a.Annotation);
    }

    public IEnumerable<(string Annotation, IEnumerable<string> Imports)> GetDomainAnnotationsAndImports(IProperty property, string tag)
    {
        if (property.Domain is not null)
        {
            foreach (var annotation in GetImplementation(property.Domain)!.Annotations
               .Where(a => FilterAnnotations(a, property, tag)))
            {
                yield return (Annotation: annotation.Text.ParseTemplate(property, this, tag), Imports: annotation.Imports.Select(i => i.ParseTemplate(property, this, tag)));
            }
        }
    }

    public IEnumerable<string> GetDomainImports(IProperty property, string tag)
    {
        if (property.Domain != null)
        {
            foreach (var import in GetImplementation(property.Domain)!.Imports.Select(u => u.ParseTemplate(property, this, tag)))
            {
                yield return import;
            }

            foreach (var import in GetDomainAnnotationsAndImports(property, tag).SelectMany(a => a.Imports))
            {
                yield return import;
            }
        }
    }

    public string GetEnumType(IProperty fp, bool isPrimaryKeyDef = false)
    {
        var op = fp switch
        {
            AssociationProperty a => a.Property,
            AliasProperty { Property: AssociationProperty a } => a.Property,
            AliasProperty alp => alp.Property,
            _ => fp
        };

        return op is AssociationProperty ap
            ? GetEnumType(ap.Association.Name, ap.Property.Name, isPrimaryKeyDef)
            : op is RegularProperty rp
            ? GetEnumType(rp.Class?.Name ?? string.Empty, rp.Name, isPrimaryKeyDef)
            : string.Empty;
    }

    /// <summary>
    /// Récupère l'implémentation du domaine pour la config.
    /// </summary>
    /// <param name="domain">Décorateur.</param>
    /// <returns>Implémentation.</returns>
    public DomainImplementation? GetImplementation(Domain? domain)
    {
        return domain?.Implementations.GetValueOrDefault(Language);
    }

    /// <summary>
    /// Récupère l'implémentation du décorateur pour la config.
    /// </summary>
    /// <param name="decorator">Décorateur.</param>
    /// <returns>Implémentation.</returns>
    public DecoratorImplementation? GetImplementation(Decorator? decorator)
    {
        return decorator?.Implementations.GetValueOrDefault(Language);
    }

    /// <summary>
    /// Récupère l'implémentation du convertisseur pour la config.
    /// </summary>
    /// <param name="converter">Convertisseur.</param>
    /// <returns>Implémentation.</returns>
    public ConverterImplementation? GetImplementation(Converter? converter)
    {
        return converter?.Implementations.GetValueOrDefault(Language);
    }

    /// <summary>
    /// Récupère le type d'une propriété.
    /// </summary>
    /// <param name="property">Domaine.</param>
    /// <param name="availableClasses">Classes disponibles.</param>
    /// <param name="useClassForAssociation">Utilise le type de la classe pour une association.</param>
    /// <returns>Le type.</returns>
    public string GetType(IProperty property, IEnumerable<Class>? availableClasses = null, bool useClassForAssociation = false)
    {
        string GetEnum(string className, string propName, bool isPrimaryKeyDef = false)
        {
            var op = property switch
            {
                AssociationProperty ap => ap.Property,
                AliasProperty { Property: AssociationProperty ap } => ap.Property,
                AliasProperty alp => alp.Property,
                _ => property
            };

            return (GetImplementation(op.Domain)?.GenericType ?? "{T}").Replace("{T}", GetEnumType(className, propName, isPrimaryKeyDef)).ParseTemplate(op, this);
        }

        string GetTransformed(string type)
        {
            var domain = GetImplementation(property.Domain);
            return (domain?.GenericType?.Replace("{T}", type) ?? domain?.Type ?? string.Empty).ParseTemplate(property, this);
        }

        string HandleAUC(AssociationProperty ap)
        {
            return ap.Property.Domain != ap.Domain
                ? GetTransformed(ap.Association.NamePascal)
                : ap.Association.NamePascal;
        }

        string HandleEnum(IProperty op)
        {
            var type = op is AssociationProperty ap
                ? GetEnum(ap.Association.Name, ap.Property.Name)
                : op is RegularProperty rp
                ? GetEnum(rp.Class.Name, rp.Name, rp == property)
                : throw new InvalidOperationException();

            if (property.Domain != (op is AssociationProperty ap2 ? ap2.Property.Domain : op.Domain))
            {
                return GetTransformed(type);
            }
            else
            {
                return type;
            }
        }

        return property switch
        {
            AssociationProperty ap when useClassForAssociation => HandleAUC(ap),
            AliasProperty { Property: AssociationProperty ap } when useClassForAssociation => HandleAUC(ap),
            AssociationProperty ap when CanClassUseEnums(ap.Association, availableClasses, ap.Property) => HandleEnum(ap),
            AliasProperty { Property: AssociationProperty ap } when CanClassUseEnums(ap.Association, availableClasses, ap.Property) => HandleEnum(ap),
            RegularProperty { Class: not null } rp when CanClassUseEnums(rp.Class, availableClasses, rp) => HandleEnum(rp),
            AliasProperty { Property: RegularProperty { Class: not null } rp } when CanClassUseEnums(rp.Class, availableClasses, rp) => HandleEnum(rp),
            AliasProperty { As: not null } alp when GetImplementation(alp.Domain)?.GenericType != null => GetImplementation(alp.Domain)!.GenericType!.Replace("{T}", GetType(alp.OriginalProperty!, availableClasses, useClassForAssociation)),
            CompositionProperty { Domain: not null } => (GetImplementation(property.Domain)?.GenericType ?? "{T}").Replace("{T}", "{composition.name}").ParseTemplate(property, this),
            AliasProperty { Property: CompositionProperty { Domain: not null } } => (GetImplementation(property.Domain)?.GenericType ?? "{T}").Replace("{T}", "{composition.name}").ParseTemplate(property, this),
            CompositionProperty cp => cp.Composition.NamePascal,
            AliasProperty { Property: CompositionProperty cp } => cp.Composition.NamePascal,
            IProperty => (GetImplementation(property.Domain)?.Type ?? string.Empty).ParseTemplate(property, this),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Récupère la valeur d'une propriété.
    /// </summary>
    /// <param name="property">La propriété.</param>
    /// <param name="availableClasses">Classes disponibles dans le générateur.</param>
    /// <param name="value">Valeur à utiliser, si non renseigné utilise la valeur par défaut de la propriété.</param>
    /// <returns>La valeur.</returns>
    public virtual string GetValue(IProperty property, IEnumerable<Class> availableClasses, string? value = null)
    {
        if (!IgnoreDefaultValues && property is not CompositionProperty and not AliasProperty { Property: CompositionProperty })
        {
            value ??= property?.DefaultValue;
        }

        if (property == null || value == null || value == "null" || value == "undefined")
        {
            return NullValue;
        }

        var template = GetImplementation(property.Domain)?.GetValueTemplate(value);
        if (template != null)
        {
            return template.Value.Replace("{value}", value).ParseTemplate(property, this);
        }

        var prop = property is AliasProperty alp ? alp.Property : property;
        var ap = prop as AssociationProperty;

        var classe = ap != null ? ap.Association : prop.Class;
        var targetProp = ap != null ? ap.Property : prop;

        if (UseNamedEnums && classe != null && classe.Enum && availableClasses.Contains(classe))
        {
            if (CanClassUseEnums(classe, availableClasses, targetProp))
            {
                return $"{GetEnumType(classe.NamePascal, targetProp.NamePascal).TrimEnd('?')}.{value}";
            }
            else if (classe.EnumKey == targetProp)
            {
                var refName = classe.Values.SingleOrDefault(rv => rv.Value[targetProp] == value)?.Name;
                if (refName != null)
                {
                    return GetConstEnumName(classe.Name, refName);
                }
            }
        }

        if (ShouldQuoteValue(property))
        {
            return QuoteValue(value);
        }

        return value;
    }

    public IEnumerable<string> GetValueImports(IProperty property, string? value = null)
    {
        if (!IgnoreDefaultValues && property is not CompositionProperty and not AliasProperty { Property: CompositionProperty })
        {
            value ??= property.DefaultValue;
        }

        var template = value != null ? GetImplementation(property.Domain)?.GetValueTemplate(value) : null;
        if (template != null)
        {
            return template.Imports.Select(i => i.ParseTemplate(property, this));
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Initialise les variables globales, et par tag manquantes.
    /// </summary>
    /// <param name="app">Valeur de la variable 'app'.</param>
    /// <param name="number">Numéro du générateur.</param>
    public void InitVariables(string app, int number)
    {
        if (!Variables.ContainsKey("app"))
        {
            Variables["app"] = app;
        }

        // Si on a défini au moins une variable par tag, alors on s'assure qu'elle est définie pour tous les tags (et on y met "" si ce n'est pas une variable globale).
        if (TagVariableNames.Any())
        {
            foreach (var tag in Tags)
            {
                if (!TagVariables.ContainsKey(tag))
                {
                    TagVariables[tag] = [];
                }
            }

            foreach (var variables in TagVariables.Values)
            {
                foreach (var varName in TagVariableNames)
                {
                    if (!variables.ContainsKey(varName))
                    {
                        Variables.TryGetValue(varName, out var globalVariable);
                        variables[varName] = globalVariable ?? string.Empty;
                    }
                }
            }
        }

        var hasMissingVar = false;
        foreach (var property in GetType().GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanWrite))
        {
            var value = (string?)property.GetValue(this);
            if (value != null)
            {
                value = ResolveGlobalVariables(value);
                property.SetValue(this, value);

                foreach (var match in Regex.Matches(value, @"\{([$a-zA-Z0-9_-]+)(:\w+)?\}").Cast<Match>())
                {
                    var varName = match.Groups[1].Value;
                    if (TemplateAttributes.TryGetValue(property.Name, out var ta) && ta.Contains(varName))
                    {
                        continue;
                    }

                    if (varName == "module" || varName == "lang" || varName == "fileName")
                    {
                        var supportedProperties = varName switch
                        {
                            "module" => PropertiesWithModuleVariableSupport,
                            "lang" => PropertiesWithLangVariableSupport,
                            "fileName" => PropertiesWithFileNameVariableSupport,
                            _ => null!
                        };

                        if (!supportedProperties.Contains(property.Name))
                        {
                            hasMissingVar = true;
                            AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Warning} {{{GetType().Name}[[{number}]].{property.Name}}} - La variable '{{{varName}}}' n'est pas supportée par cette propriété.[/]");
                        }

                        continue;
                    }

                    var hasTagSupport = PropertiesWithTagVariableSupport.Contains(property.Name);

                    if (!hasTagSupport)
                    {
                        hasMissingVar = true;
                        AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Warning} {{{GetType().Name}[[{number}]].{property.Name}}} - La variable globale '{{{varName}}}' n'est pas définie pour ce générateur.[/]");
                    }
                    else if (!TagVariableNames.Contains(varName))
                    {
                        hasMissingVar = true;
                        AnsiConsole.MarkupLine($"[yellow]{Emoji.Known.Warning}  {{{GetType().Name}[[{number}]].{property.Name}}} - La variable '{{{varName}}}' n'est pas définie pour ce générateur.[/]");
                    }
                }
            }
        }

        foreach (var tagVariables in TagVariables.Values)
        {
            foreach (var tagVarName in tagVariables.Keys)
            {
                foreach (var varName in GlobalVariableNames)
                {
                    tagVariables[tagVarName] = ReplaceVariable(tagVariables[tagVarName], varName, Variables[varName]);
                }
            }
        }

        if (hasMissingVar)
        {
            AnsiConsole.WriteLine();
        }
    }

    public virtual bool IsPersistent(Class classe, string tag)
    {
        return classe.IsPersistent;
    }

    public string ResolveGlobalVariables(string input)
    {
        foreach (var varName in GlobalVariableNames)
        {
            input = ReplaceVariable(input, varName, Variables[varName]);
        }

        return input;
    }

    /// <summary>
    /// Résout toutes les variables pour une valeur donnée.
    /// </summary>
    /// <param name="value">Valeur.</param>
    /// <param name="tag">Tag.</param>
    /// <param name="module">Module.</param>
    /// <param name="lang">Lang.</param>
    /// <returns>La valeur avec les variables résolues.</returns>
    public virtual string ResolveVariables(string value, string? tag = null, string? module = null, string? lang = null)
    {
        var result = value;

        if (tag != null)
        {
            result = ResolveTagVariables(result, tag);
        }

        if (module != null)
        {
            result = ReplaceVariable(result, "module", module);
        }

        if (lang != null)
        {
            result = ReplaceVariable(result, "lang", lang);
        }

        return result;
    }

    /// <summary>
    /// Détermine si une valeur de cette propriété doit être mise entre guillemets.
    /// </summary>
    /// <param name="property">Propriété.</param>
    /// <returns>Oui/non.</returns>
    public virtual bool ShouldQuoteValue(IProperty property)
    {
        return GetImplementation(property.Domain)?.Type?.ToLower() == "string";
    }

    protected virtual string GetConstEnumName(string className, string refName)
    {
        return $"{className.ToPascalCase(strictIfUppercase: true)}.{refName.ToPascalCase(strictIfUppercase: true)}";
    }

    protected abstract string GetEnumType(string className, string propName, bool isPrimaryKeyDef = false);

    protected virtual bool IsEnumNameValid(string name)
    {
        return !Regex.IsMatch(name ?? string.Empty, "^\\d");
    }

    protected virtual string QuoteValue(string value)
    {
        return $@"""{value}""";
    }

    /// <summary>
    /// Résout les variables de tag dans une chaîne de caractère.
    /// </summary>
    /// <param name="value">Chaîne de caractères.</param>
    /// <param name="tag">Nom du tag.</param>
    /// <returns>Value avec les variables remplacées..</returns>
    protected virtual string ResolveTagVariables(string value, string tag)
    {
        if (TagVariables.TryGetValue(tag, out var tagVariables))
        {
            foreach (var (varName, varValue) in tagVariables)
            {
                value = ReplaceVariable(value, varName, varValue);
            }
        }

        return value;
    }

    private static string ReplaceVariable(string value, string varName, string varValue)
    {
        string MatchEvaluator(Match m) => m.Value.Trim('{', '}').GetTransformation()(varValue);
        return Regex.Replace(value, $"\\{{{varName}(:\\w+)?\\}}", MatchEvaluator);
    }

    private bool FilterAnnotations(TargetedText annotation, IProperty property, string tag)
    {
        return property.Class != null && !property.Class.Abstract && (
            (annotation.Target & Target.Dto) > 0 && !IsPersistent(property.Class, tag)
            || (annotation.Target & Target.Persisted) > 0 && IsPersistent(property.Class, tag))
        || (annotation.Target & Target.Api) > 0 && property.Endpoint != null;
    }
}