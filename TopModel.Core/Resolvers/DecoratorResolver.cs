using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Core.Resolvers;

internal class DecoratorResolver(ModelFile modelFile, IDictionary<string, Decorator> referencedDecorators)
{
    /// <summary>
    /// Recopie les propriétés de décorateurs sur les classes et les endpoints.
    /// </summary>
    public void CopyDecoratorProperties()
    {
        foreach (var classe in modelFile.Classes)
        {
            if (classe.Decorators.Count > 0)
            {
                foreach (var prop in classe.Properties.Where(p => p.Decorator is not null).ToList())
                {
                    classe.Properties.Remove(prop);
                }

                foreach (var prop in classe.Decorators.SelectMany(d => d.Decorator.Properties))
                {
                    classe.Properties.Add(prop.CloneWithClassOrEndpoint(classe: classe));
                }
            }
        }

        foreach (var endpoint in modelFile.Endpoints)
        {
            if (endpoint.Decorators.Count > 0)
            {
                foreach (var prop in endpoint.Params.Where(p => p.Decorator is not null).ToList())
                {
                    endpoint.Params.Remove(prop);
                }

                foreach (var prop in endpoint.Decorators.SelectMany(d => d.Decorator.Properties))
                {
                    endpoint.Params.Add(prop.CloneWithClassOrEndpoint(endpoint: endpoint));
                }
            }
        }
    }

    /// <summary>
    /// Résout les décorateurs sur les classes et les endpoints.
    /// </summary>
    /// <returns>Erreurs.</returns>
    public IEnumerable<ModelError> ResolveDecorators()
    {
        foreach (var classe in modelFile.Classes.Where(c => c.DecoratorReferences.Count > 0))
        {
            classe.Decorators.Clear();

            var isError = false;
            foreach (var decoratorRef in classe.DecoratorReferences)
            {
                if (!referencedDecorators.TryGetValue(decoratorRef.ReferenceName, out var decorator))
                {
                    isError = true;
                    yield return new ModelError(classe, $"Le décorateur '{decoratorRef.ReferenceName}' est introuvable dans le fichier ou l'une de ses dépendances.", decoratorRef) { ModelErrorType = ModelErrorType.TMD1008 };
                }
                else
                {
                    if (classe.Decorators.Any(d => d.Decorator == decorator))
                    {
                        isError = true;
                        yield return new ModelError(classe, $"Le décorateur '{decoratorRef.ReferenceName}' est déjà présent dans la liste des décorateurs de la classe '{classe}'.", decoratorRef) { ModelErrorType = ModelErrorType.TMD1009 };
                    }
                    else
                    {
                        if (decorator.Implementations.Any(impl => impl.Value.Extends != null && (classe.Extends != null || classe.Decorators.Any(d => d.Decorator.Implementations.TryGetValue(impl.Key, out var dImpl) && dImpl.Extends != null))))
                        {
                            isError = true;
                            yield return new ModelError(classe, $"Impossible d'appliquer le décorateur '{decoratorRef.ReferenceName}' à la classe '{classe}' : seul un 'extends' peut être spécifié.", decoratorRef) { ModelErrorType = ModelErrorType.TMD1010 };
                        }

                        classe.Decorators.Add((decorator, decoratorRef.ParameterReferences.Select(p => p.ReferenceName).ToArray()));
                    }
                }
            }

            if (isError)
            {
                continue;
            }
        }

        foreach (var endpoint in modelFile.Endpoints.Where(c => c.DecoratorReferences.Count > 0))
        {
            endpoint.Decorators.Clear();

            var isError = false;
            foreach (var decoratorRef in endpoint.DecoratorReferences)
            {
                if (!referencedDecorators.TryGetValue(decoratorRef.ReferenceName, out var decorator))
                {
                    isError = true;
                    yield return new ModelError(endpoint, $"Le décorateur '{decoratorRef.ReferenceName}' est introuvable dans le fichier ou l'une de ses dépendances.", decoratorRef) { ModelErrorType = ModelErrorType.TMD1008 };
                }
                else
                {
                    if (endpoint.Decorators.Any(d => d.Decorator == decorator))
                    {
                        isError = true;
                        yield return new ModelError(endpoint, $"Le décorateur '{decoratorRef.ReferenceName}' est déjà présent dans la liste des décorateurs du endpoint '{endpoint}'.", decoratorRef) { ModelErrorType = ModelErrorType.TMD1009 };
                    }
                    else
                    {
                        endpoint.Decorators.Add((decorator, decoratorRef.ParameterReferences.Select(p => p.ReferenceName).ToArray()));
                    }
                }
            }

            if (isError)
            {
                continue;
            }
        }
    }
}
