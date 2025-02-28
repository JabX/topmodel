////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models.CSharp.Securite.Profil.Models;

namespace CSharp.Clients.External.Securite.Profil;

/// <summary>
/// Client Profil.
/// </summary>
/// <param name="client">HttpClient injecté.</param>
public partial class ProfilClient(HttpClient client)
{
    private readonly JsonSerializerOptions _jsOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Ajoute un Profil.
    /// </summary>
    /// <param name="profil">Profil à sauvegarder.</param>
    /// <returns>Profil sauvegardé.</returns>
    public async Task<ProfilRead> AddProfil(ProfilWrite profil)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Post, $"api/profils") { Content = JsonContent.Create(profil, options: _jsOptions) }, HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<ProfilRead>(_jsOptions);
    }

    /// <summary>
    /// Charge le détail d'un Profil.
    /// </summary>
    /// <param name="proId">Id technique.</param>
    /// <returns>Le détail de l'Profil.</returns>
    public async Task<ProfilRead> GetProfil(int proId)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Get, $"api/profils/{proId}"), HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<ProfilRead>(_jsOptions);
    }

    /// <summary>
    /// Liste tous les Profils.
    /// </summary>
    /// <returns>Profils matchant les critères.</returns>
    public async Task<ICollection<ProfilItem>> GetProfils()
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Get, $"api/profils"), HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<ICollection<ProfilItem>>(_jsOptions);
    }

    /// <summary>
    /// Sauvegarde un Profil.
    /// </summary>
    /// <param name="proId">Id technique.</param>
    /// <param name="profil">Profil à sauvegarder.</param>
    /// <returns>Profil sauvegardé.</returns>
    public async Task<ProfilRead> UpdateProfil(int proId, ProfilWrite profil)
    {
        await EnsureAuthentication();
        using var res = await client.SendAsync(new(HttpMethod.Put, $"api/profils/{proId}") { Content = JsonContent.Create(profil, options: _jsOptions) }, HttpCompletionOption.ResponseHeadersRead);
        await EnsureSuccess(res);

        return await res.Content.ReadFromJsonAsync<ProfilRead>(_jsOptions);
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
