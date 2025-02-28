////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using System.ComponentModel.DataAnnotations;
using CSharp.Common;
using Kinetix.Modeling.Annotations;
using Models.CSharp.Securite.Utilisateur.Models;

namespace Models.CSharp.Securite.Profil.Models;

/// <summary>
/// Détail d'un profil en lecture.
/// </summary>
public partial record ProfilRead
{
    /// <summary>
    /// Id technique.
    /// </summary>
    [Required]
    [Domain(Domains.Id)]
    public int? Id { get; set; }

    /// <summary>
    /// Libellé du profil.
    /// </summary>
    [Required]
    [Domain(Domains.Libelle)]
    [StringLength(100)]
    public string Libelle { get; set; }

    /// <summary>
    /// Liste des droits du profil.
    /// </summary>
    [ReferencedType(typeof(Droit))]
    [Domain(Domains.CodeListe)]
    public Droit.Codes[] Droits { get; set; }

    /// <summary>
    /// Date de création de l'utilisateur.
    /// </summary>
    [Required]
    [Domain(Domains.DateHeure)]
    public DateTime? DateCreation { get; set; }

    /// <summary>
    /// Date de modification de l'utilisateur.
    /// </summary>
    [Domain(Domains.DateHeure)]
    public DateTime? DateModification { get; set; }

    /// <summary>
    /// Utilisateurs ayant ce profil.
    /// </summary>
    [Required]
    public ICollection<UtilisateurItem> Utilisateurs { get; set; } = new List<UtilisateurItem>();
}
