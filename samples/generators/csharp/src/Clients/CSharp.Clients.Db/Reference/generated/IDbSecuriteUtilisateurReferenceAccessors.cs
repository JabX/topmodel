////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using Kinetix.Services.Annotations;
using Models.CSharp.Securite.Utilisateur.Models;

namespace CSharp.Clients.Db.Reference;

/// <summary>
/// Accesseurs de listes de référence persistées.
/// </summary>
[RegisterContract]
public partial interface IDbSecuriteUtilisateurReferenceAccessors
{
    /// <summary>
    /// Accesseur de référence pour le type TypeUtilisateur.
    /// </summary>
    /// <returns>Liste de TypeUtilisateur.</returns>
    [ReferenceAccessor]
    ICollection<TypeUtilisateur> LoadTypeUtilisateurs();
}
