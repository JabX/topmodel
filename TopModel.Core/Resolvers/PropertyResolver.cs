using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Core.Resolvers;

internal class PropertyResolver(ModelFile modelFile, IDictionary<string, Domain> domains, IDictionary<string, Class> referencedClasses)
{
    /// <summary>
    /// Réinitialise les alias déjà résolus sur les classes/endpoints/décorateurs/mappers (pour le watch).
    /// </summary>
    public void ResetAliases()
    {
        foreach (var classe in modelFile.Classes)
        {
            foreach (var alp in classe.Properties.OfType<AliasProperty>().ToList())
            {
                if (alp.OriginalAliasProperty is not null)
                {
                    var index = classe.Properties.IndexOf(alp);
                    classe.Properties.RemoveAt(index);
                    if (!classe.Properties.Contains(alp.OriginalAliasProperty))
                    {
                        classe.Properties.Insert(index, alp.OriginalAliasProperty);
                    }
                }
            }

            foreach (var alp in classe.FromMapperProperties.OfType<AliasProperty>().ToList())
            {
                if (alp.OriginalAliasProperty is not null)
                {
                    var index = alp.PropertyMapping.FromMapper.Params.IndexOf(alp.PropertyMapping);
                    alp.PropertyMapping.FromMapper.Params.RemoveAt(index);
                    if (!alp.PropertyMapping.FromMapper.Params.Contains(alp.OriginalAliasProperty.PropertyMapping))
                    {
                        alp.PropertyMapping.FromMapper.Params.Insert(index, new PropertyMapping
                        {
                            FromMapper = alp.PropertyMapping.FromMapper,
                            Property = alp.OriginalAliasProperty,
                            TargetProperty = alp.PropertyMapping.TargetProperty,
                            TargetPropertyReference = alp.PropertyMapping.TargetPropertyReference
                        });
                    }
                }
            }
        }

        foreach (var endpoint in modelFile.Endpoints)
        {
            foreach (var alp in endpoint.Params.OfType<AliasProperty>().ToList())
            {
                if (alp.OriginalAliasProperty is not null)
                {
                    var index = endpoint.Params.IndexOf(alp);
                    endpoint.Params.RemoveAt(endpoint.Params.IndexOf(alp));
                    if (!endpoint.Params.Contains(alp.OriginalAliasProperty))
                    {
                        endpoint.Params.Insert(index, alp.OriginalAliasProperty);
                    }
                }
            }

            if (endpoint.Returns is AliasProperty ralp && ralp.OriginalAliasProperty is not null)
            {
                endpoint.Returns = ralp.OriginalAliasProperty;
            }
        }

        foreach (var decorator in modelFile.Decorators)
        {
            foreach (var alp in decorator.Properties.OfType<AliasProperty>().ToList())
            {
                if (alp.OriginalAliasProperty is not null)
                {
                    var index = decorator.Properties.IndexOf(alp);
                    decorator.Properties.RemoveAt(decorator.Properties.IndexOf(alp));
                    if (!decorator.Properties.Contains(alp.OriginalAliasProperty))
                    {
                        decorator.Properties.Insert(index, alp.OriginalAliasProperty);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Résout les alias d'un fichier.
    /// </summary>
    /// <param name="filter">Aliases à prendre en compte dans le fichier.</param>
    /// <returns>Erreurs.</returns>
    public IEnumerable<ModelError> ResolveAliases(Func<AliasProperty, bool> filter)
    {
        foreach (var alp in modelFile.Properties.OfType<AliasProperty>().Where(filter))
        {
            if (!referencedClasses!.TryGetValue(alp.Reference!.ReferenceName, out var aliasedClass))
            {
                yield return new ModelError(alp, "La classe '{0}' est introuvable dans le fichier ou l'une de ses dépendances.", alp.Reference) { ModelErrorType = ModelErrorType.TMD1002 };
                continue;
            }

            var shouldBreak = false;
            foreach (var propReference in alp.Reference.IncludeReferences.Concat(alp.Reference.ExcludeReferences))
            {
                var aliasedProperty = aliasedClass.Properties.FirstOrDefault(p => p.Name == propReference.ReferenceName);
                if (aliasedProperty == null)
                {
                    yield return new ModelError(alp, $"La propriété '{{0}}' est introuvable sur la classe '{aliasedClass}'.", propReference) { ModelErrorType = ModelErrorType.TMD1004 };
                    shouldBreak = true;
                }
            }

            foreach (var include in alp.Reference.IncludeReferences.Where((e, i) => alp.Reference.IncludeReferences.Where((p, j) => p.ReferenceName == e.ReferenceName && j < i).Any()))
            {
                yield return new ModelError(modelFile, $"La propriété '{include.ReferenceName}' est déjà référencée dans la définition de l'alias.", include) { IsError = true, ModelErrorType = ModelErrorType.TMD0004 };
                shouldBreak = true;
            }

            foreach (var exclude in alp.Reference.ExcludeReferences.Where((e, i) => alp.Reference.ExcludeReferences.Where((p, j) => p.ReferenceName == e.ReferenceName && j < i).Any()))
            {
                yield return new ModelError(modelFile, $"La propriété '{exclude.ReferenceName}' est déjà référencée dans la définition de l'alias.", exclude) { IsError = true, ModelErrorType = ModelErrorType.TMD0004 };
                shouldBreak = true;
            }

            if (shouldBreak)
            {
                continue;
            }

            var propertiesToAlias =
                (alp.Reference.IncludeReferences.Count > 0
                    ? alp.Reference.IncludeReferences.Select(p => aliasedClass.Properties.First(prop => prop.Name == p.ReferenceName))
                    : aliasedClass.Properties.Where(prop => !alp.Reference.ExcludeReferences.Select(p => p.ReferenceName).Contains(prop.Name)))
                .Reverse();

            foreach (var property in propertiesToAlias)
            {
                var prop = alp.Clone(property, alp.Reference.IncludeReferences.FirstOrDefault(ir => ir.ReferenceName == property.Name));

                if (prop.As != null && prop.Domain == null)
                {
                    yield return new ModelError(modelFile, $"Le domaine '{prop.OriginalProperty?.Domain}' doit définir un domaine 'as' pour '{prop.As}' pour définir un alias '{prop.As}' sur la propriété '{prop.OriginalProperty}' de la classe '{prop.OriginalProperty?.Class}'", prop.PropertyReference ?? prop.Reference) { IsError = true, ModelErrorType = ModelErrorType.TMD1023 };
                }

                if (alp.Class != null)
                {
                    var index = alp.Class.Properties.IndexOf(alp);
                    if (index >= 0)
                    {
                        alp.Class.Properties.Insert(index + 1, prop);
                    }
                }
                else if (alp.Endpoint?.Params.Contains(alp) ?? false)
                {
                    var index = alp.Endpoint.Params.IndexOf(alp);
                    if (index >= 0)
                    {
                        alp.Endpoint.Params.Insert(index + 1, prop);
                    }
                }
                else if (alp.Endpoint?.Returns == alp)
                {
                    alp.Endpoint.Returns = prop;
                }
                else if (alp.Decorator != null)
                {
                    var index = alp.Decorator.Properties.IndexOf(alp);
                    if (index >= 0)
                    {
                        alp.Decorator.Properties.Insert(index + 1, prop);
                    }
                }
                else if (alp.PropertyMapping != null)
                {
                    var index = alp.PropertyMapping.FromMapper.Params.FindIndex(param => param.TryPickT1(out var pm, out var _) && pm.Property == alp);
                    if (index >= 0)
                    {
                        var mapping = new PropertyMapping
                        {
                            FromMapper = alp.PropertyMapping.FromMapper,
                            Property = prop,
                            TargetProperty = alp.PropertyMapping.TargetProperty,
                            TargetPropertyReference = alp.PropertyMapping.TargetPropertyReference,
                        };
                        prop.PropertyMapping = mapping;
                        alp.PropertyMapping.FromMapper.Params.Insert(index + 1, mapping);
                    }
                }
            }

            if (alp.Class != null)
            {
                alp.Class.Properties.Remove(alp);
            }
            else if (alp.Endpoint?.Params.Contains(alp) ?? false)
            {
                alp.Endpoint.Params.Remove(alp);
            }
            else if (alp.PropertyMapping != null)
            {
                alp.PropertyMapping.FromMapper.Params.RemoveAll(param => param.TryPickT1(out var pm, out var _) && pm.Property == alp);
            }
            else
            {
                alp.Decorator?.Properties.Remove(alp);
            }
        }
    }

    /// <summary>
    /// Résout les propriétés cible (la FK) pour les associations.
    /// </summary>
    /// <returns>Erreurs.</returns>
    public IEnumerable<ModelError> ResolveAssociationProperties()
    {
        foreach (var ap in modelFile.Classes.SelectMany(c => c.Properties.OfType<AssociationProperty>()).Where(ap => ap.Association != null))
        {
            if (ap.Type.IsToMany() && !(ap.Property?.Domain?.AsDomains.ContainsKey(ap.As) ?? false))
            {
                yield return new ModelError(ap, $@"Cette association ne peut pas avoir le type {ap.Type} car le domain {ap.Property?.Domain} ne contient pas de définition de domaine 'as' pour '{ap.As}'.", ap.Reference) { ModelErrorType = ModelErrorType.TMD1028 };
                continue;
            }

            if (ap.PropertyReference != null)
            {
                var referencedProperty = ap.Association.ExtendedProperties.FirstOrDefault(p => p.Name == ap.PropertyReference!.ReferenceName);
                if (referencedProperty == null)
                {
                    yield return new ModelError(ap, $"La propriété '{{0}}' est introuvable sur la classe '{ap.Association}'.", ap.PropertyReference) { ModelErrorType = ModelErrorType.TMD1004 };
                }

                ap.Property = referencedProperty;
            }
        }
    }

    /// <summary>
    /// Résolutions des références sur les propriétés (hors alias).
    /// On ne touche pas aux propriétés liées à une classe et un décorateur en même temps car
    /// ces propriétés sont déjà résolues sur les décorateurs avant d'être recopiées sur les classes.
    /// </summary>
    /// <returns>Erreurs.</returns>
    public IEnumerable<ModelError> ResolveNonAliasProperties()
    {
        foreach (var prop in modelFile.Properties.Where(p => p.Decorator is null || p.Class is null))
        {
            switch (prop)
            {
                case RegularProperty rp:
                    if (rp.DomainReference == null || !domains.TryGetValue(rp.DomainReference.ReferenceName, out var domain))
                    {
                        yield return new ModelError(rp, "Le domaine '{0}' est introuvable.", rp.DomainReference) { ModelErrorType = ModelErrorType.TMD1005 };
                        break;
                    }

                    rp.Domain = domain;
                    rp.DomainParameters = rp.DomainReference.ParameterReferences.Select(p => p.ReferenceName).ToArray();
                    break;

                case AssociationProperty ap:
                    if (!referencedClasses.TryGetValue(ap.Reference.ReferenceName, out var association))
                    {
                        yield return new ModelError(ap, "La classe '{0}' est introuvable dans le fichier ou l'une de ses dépendances.", ap.Reference) { ModelErrorType = ModelErrorType.TMD1002 };
                        break;
                    }

                    if (ap.PropertyReference == null && !association.ExtendedProperties.Any(p => p.PrimaryKey))
                    {
                        yield return new ModelError(ap, "La classe '{0}' doit avoir au moins une clé primaire pour être référencée dans une association.", ap.Reference) { ModelErrorType = ModelErrorType.TMD1001 };
                        break;
                    }

                    if (ap.PropertyReference == null && association.Properties.Count(p => p.PrimaryKey) > 1 && ap.PropertyReference == null)
                    {
                        yield return new ModelError(ap, "La classe '{0}' a plusieurs clés primaires, vous devez obligatoirement référencer une propriété cible.", ap.Reference) { ModelErrorType = ModelErrorType.TMD1001 };
                        break;
                    }

                    ap.Association = association;
                    break;

                case CompositionProperty cp:
                    if (!referencedClasses.TryGetValue(cp.Reference.ReferenceName, out var composition))
                    {
                        yield return new ModelError(cp, "La classe '{0}' est introuvable dans le fichier ou l'une de ses dépendances.", cp.Reference) { ModelErrorType = ModelErrorType.TMD1002 };
                        break;
                    }

                    cp.Composition = composition;

                    if (cp.DomainReference != null)
                    {
                        if (!domains.TryGetValue(cp.DomainReference.ReferenceName, out var cpDomain))
                        {
                            yield return new ModelError(cp, "Le domaine '{0}' est introuvable.", cp.DomainReference) { ModelErrorType = ModelErrorType.TMD1005 };
                            break;
                        }

                        cp.Domain = cpDomain;
                        cp.DomainParameters = cp.DomainReference.ParameterReferences.Select(p => p.ReferenceName).ToArray();
                    }

                    break;

                case AliasProperty alp when alp.DomainReference != null:
                    if (!domains.TryGetValue(alp.DomainReference.ReferenceName, out var aliasDomain))
                    {
                        yield return new ModelError(alp, "Le domaine '{0}' est introuvable.", alp.DomainReference) { ModelErrorType = ModelErrorType.TMD1005 };
                        break;
                    }

                    alp.Domain = aliasDomain;
                    alp.DomainParameters = alp.DomainReference.ParameterReferences.Select(p => p.ReferenceName).ToArray();
                    break;
            }
        }
    }
}
