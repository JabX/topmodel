////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models.CSharp.Securite.Utilisateur.Models;

namespace CSharp.Clients.External.Securite.Utilisateur;

/// <summary>
/// Client Utilisateur.
/// </summary>
/// <param name="client">HttpClient injecté.</param>
public partial class UtilisateurClient(HttpClient client)
{
    private readonly JsonSerializerOptions _jsOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Ajoute un utilisateur.
    /// </summary>
    /// <param name="utilisateur">Utilisateur à sauvegarder.</param>
    /// <returns>Utilisateur sauvegardé.</returns>
    public async Task<UtilisateurRead> AddUtilisateur(UtilisateurWrite utilisateur)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Post, $"api/utilisateurs") { Content = JsonContent.Create(utilisateur, options: _jsOptions) }, HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<UtilisateurRead>(_jsOptions);
    }

    /// <summary>
    /// Supprime un utilisateur.
    /// </summary>
    /// <param name="utiId">Id de l'utilisateur.</param>
    /// <returns>Task.</returns>
    public async Task DeleteUtilisateur(int utiId)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Delete, $"api/utilisateurs/{utiId}"));
        await EnsureSuccess(res);
    }

    /// <summary>
    /// Charge le détail d'un utilisateur.
    /// </summary>
    /// <param name="utiId">Id de l'utilisateur.</param>
    /// <returns>Le détail de l'utilisateur.</returns>
    public async Task<UtilisateurRead> GetUtilisateur(int utiId)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Get, $"api/utilisateurs/{utiId}"), HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<UtilisateurRead>(_jsOptions);
    }

    /// <summary>
    /// Recherche des utilisateurs.
    /// </summary>
    /// <param name="nom">Nom de l'utilisateur.</param>
    /// <param name="prenom">Nom de l'utilisateur.</param>
    /// <param name="email">Email de l'utilisateur.</param>
    /// <param name="dateNaissance">Age de l'utilisateur.</param>
    /// <param name="adresse">Adresse de l'utilisateur.</param>
    /// <param name="actif">Si l'utilisateur est actif.</param>
    /// <param name="profilId">Profil de l'utilisateur.</param>
    /// <param name="typeUtilisateurCode">Type d'utilisateur.</param>
    /// <returns>Utilisateurs matchant les critères.</returns>
    public async Task<ICollection<UtilisateurItem>> SearchUtilisateur(string nom = null, string prenom = null, string email = null, DateOnly? dateNaissance = null, string adresse = null, bool? actif = null, int? profilId = null, TypeUtilisateur.Codes? typeUtilisateurCode = null)
    {
        await EnsureAuthentication();
        var query = await new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["nom"] = nom,
            ["prenom"] = prenom,
            ["email"] = email,
            ["dateNaissance"] = dateNaissance?.ToString(CultureInfo.InvariantCulture),
            ["adresse"] = adresse,
            ["actif"] = actif?.ToString(CultureInfo.InvariantCulture),
            ["profilId"] = profilId?.ToString(CultureInfo.InvariantCulture),
            ["typeUtilisateurCode"] = typeUtilisateurCode?.ToString(CultureInfo.InvariantCulture),
        }.Where(kv => kv.Value != null)).ReadAsStringAsync();
        using var res = await client.SendAsync(new(HttpMethod.Get, $"api/utilisateurs?{query}"), HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<ICollection<UtilisateurItem>>(_jsOptions);
    }

    /// <summary>
    /// Sauvegarde un utilisateur.
    /// </summary>
    /// <param name="utiId">Id de l'utilisateur.</param>
    /// <param name="utilisateur">Utilisateur à sauvegarder.</param>
    /// <returns>Utilisateur sauvegardé.</returns>
    public async Task<UtilisateurRead> UpdateUtilisateur(int utiId, UtilisateurWrite utilisateur)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Put, $"api/utilisateurs/{utiId}") { Content = JsonContent.Create(utilisateur, options: _jsOptions) }, HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<UtilisateurRead>(_jsOptions);
    }

    /// <summary>
    /// Assure que l'authentification est configurée.
    /// </summary>
    private partial Task EnsureAuthentication();

    /// <summary>
    /// Gère les erreurs éventuelles retournées par l'API appelée.
    /// </summary>
    /// <param name="response">Réponse HTTP.</param>
    private partial Task EnsureSuccess(HttpResponseMessage response);
}
