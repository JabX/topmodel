using TopModel.Core.FileModel;
using TopModel.Utils;

namespace TopModel.Core.Resolvers;

internal class EndpointResolver(ModelFile modelFile)
{
    /// <summary>
    /// Effectue les vérifications de cohérence sur le résultat de la résolution des endpoints.
    /// </summary>
    /// <returns>Erreurs.</returns>
    public IEnumerable<ModelError> CheckResult()
    {
        foreach (var endpoint in modelFile.Endpoints)
        {
            foreach (var queryParam in endpoint.GetQueryAndMultipartParams())
            {
                var index = endpoint.Params.IndexOf(queryParam);

                if (endpoint.Params.Any(param => !param.IsQueryOrMultipartParam() && endpoint.Params.IndexOf(param) > index))
                {
                    yield return new ModelError(endpoint, $"Le paramètre de requête (ou multipart) '{queryParam.GetParamName()}' doit suivre tous les paramètres de route ou de body dans un endpoint.", queryParam.GetLocation()) { IsError = false, ModelErrorType = ModelErrorType.TMD9003 };
                }
            }

            var split = endpoint.FullRoute.Split("/");

            for (var i = 0; i < split.Length; i++)
            {
                if (split[i].StartsWith('{'))
                {
                    var routeParamName = split[i][1..^1];
                    var param = endpoint.Params.SingleOrDefault(param => param.GetParamName() == routeParamName);

                    if (param == null)
                    {
                        yield return new ModelError(endpoint, $"Le endpoint '{endpoint.Name}' définit un paramètre '{routeParamName}' dans sa route qui n'existe pas dans la liste des paramètres. Les valeurs possibles sont : {string.Join(", ", endpoint.Params.Select(p => p.GetParamName()))}.") { ModelErrorType = ModelErrorType.TMD1027 };
                    }
                }
            }
        }
    }
}
