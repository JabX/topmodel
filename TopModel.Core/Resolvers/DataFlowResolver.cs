using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Core.Resolvers;

internal class DataFlowResolver(ModelFile modelFile, IDictionary<string, DataFlow> referencedDataFlows, IDictionary<string, Class> referencedClasses)
{
    /// <summary>
    /// Résout les flux de données.
    /// </summary>
    /// <returns>Erreurs.</returns>
    public IEnumerable<ModelError> ResolveDataFlows()
    {
        foreach (var dataFlow in modelFile.DataFlows)
        {
            if (!referencedClasses.TryGetValue(dataFlow.ClassReference.ReferenceName, out var classe))
            {
                yield return new ModelError(dataFlow, "La classe '{0}' est introuvable dans le fichier ou l'une de ses dépendances.", dataFlow.ClassReference) { ModelErrorType = ModelErrorType.TMD1002 };
                continue;
            }

            dataFlow.Class = classe;

            if (dataFlow.ActivePropertyReference != null)
            {
                dataFlow.ActiveProperty = classe.ExtendedProperties.FirstOrDefault(fp => fp.Name == dataFlow.ActivePropertyReference.ReferenceName);
                if (dataFlow.ActiveProperty == null)
                {
                    yield return new ModelError(dataFlow, $"La propriété '{dataFlow.ActivePropertyReference.ReferenceName}' n'existe pas sur la classe '{classe}'.", dataFlow.ActivePropertyReference) { ModelErrorType = ModelErrorType.TMD1011 };
                }
            }

            dataFlow.DependsOn.Clear();

            foreach (var dependsOnReference in dataFlow.DependsOnReference)
            {
                if (!referencedDataFlows.TryGetValue(dependsOnReference.ReferenceName, out var referencedDataFlow))
                {
                    yield return new ModelError(dataFlow, "Le flux de données '{0}' est introuvable dans le fichier ou l'une de ses dépendances.", dependsOnReference) { ModelErrorType = ModelErrorType.TMD2000 };
                    continue;
                }

                dataFlow.DependsOn.Add(referencedDataFlow);
            }

            foreach (var source in dataFlow.Sources)
            {
                if (!referencedClasses.TryGetValue(source.ClassReference.ReferenceName, out var sourceClass))
                {
                    yield return new ModelError(dataFlow, "La classe '{0}' est introuvable dans le fichier ou l'une de ses dépendances.", source.ClassReference) { ModelErrorType = ModelErrorType.TMD1002 };
                    continue;
                }

                source.Class = sourceClass;
                source.JoinProperties.Clear();

                foreach (var joinPropertyReference in source.JoinPropertyReferences)
                {
                    var joinProperty = sourceClass.ExtendedProperties.FirstOrDefault(fp => fp.Name == joinPropertyReference.ReferenceName);
                    if (joinProperty == null)
                    {
                        yield return new ModelError(dataFlow, $"La propriété '{joinPropertyReference.ReferenceName}' n'existe pas sur la classe '{sourceClass}'.", joinPropertyReference) { ModelErrorType = ModelErrorType.TMD1011 };
                    }

                    source.JoinProperties.Add(joinProperty);
                }
            }
        }
    }
}
