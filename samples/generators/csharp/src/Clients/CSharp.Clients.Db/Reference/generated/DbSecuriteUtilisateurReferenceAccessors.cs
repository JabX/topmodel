////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using Kinetix.Services.Annotations;
using Models.CSharp.Securite.Utilisateur.Models;

namespace CSharp.Clients.Db.Reference;

/// <summary>
/// Implémentation de IDbSecuriteUtilisateurReferenceAccessors.
/// </summary>
/// <param name="dbContext">DbContext.</param>
[RegisterImpl]
public partial class DbSecuriteUtilisateurReferenceAccessors(CSharpDbContext dbContext) : IDbSecuriteUtilisateurReferenceAccessors
{
    /// <inheritdoc cref="IDbSecuriteUtilisateurReferenceAccessors.LoadTypeUtilisateurs" />
    public ICollection<TypeUtilisateur> LoadTypeUtilisateurs()
    {
        return dbContext.TypeUtilisateurs.OrderBy(row => row.Libelle).ToList();
    }
}
