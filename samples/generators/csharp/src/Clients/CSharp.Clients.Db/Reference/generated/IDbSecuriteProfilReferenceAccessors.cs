////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using Kinetix.Services.Annotations;
using Models.CSharp.Securite.Profil.Models;

namespace CSharp.Clients.Db.Reference;

/// <summary>
/// Accesseurs de listes de référence persistées.
/// </summary>
[RegisterContract]
public partial interface IDbSecuriteProfilReferenceAccessors
{
    /// <summary>
    /// Accesseur de référence pour le type Droit.
    /// </summary>
    /// <returns>Liste de Droit.</returns>
    [ReferenceAccessor]
    ICollection<Droit> LoadDroits();

    /// <summary>
    /// Accesseur de référence pour le type TypeDroit.
    /// </summary>
    /// <returns>Liste de TypeDroit.</returns>
    [ReferenceAccessor]
    ICollection<TypeDroit> LoadTypeDroits();
}
