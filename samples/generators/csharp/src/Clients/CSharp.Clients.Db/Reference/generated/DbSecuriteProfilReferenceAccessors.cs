////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using Kinetix.Services.Annotations;
using Models.CSharp.Securite.Profil.Models;

namespace CSharp.Clients.Db.Reference;

/// <summary>
/// Implémentation de IDbSecuriteProfilReferenceAccessors.
/// </summary>
/// <param name="dbContext">DbContext.</param>
[RegisterImpl]
public partial class DbSecuriteProfilReferenceAccessors(CSharpDbContext dbContext) : IDbSecuriteProfilReferenceAccessors
{
    /// <inheritdoc cref="IDbSecuriteProfilReferenceAccessors.LoadDroits" />
    public ICollection<Droit> LoadDroits()
    {
        return dbContext.Droits.OrderBy(row => row.Code).ToList();
    }

    /// <inheritdoc cref="IDbSecuriteProfilReferenceAccessors.LoadTypeDroits" />
    public ICollection<TypeDroit> LoadTypeDroits()
    {
        return dbContext.TypeDroits.OrderBy(row => row.Libelle).ToList();
    }
}
