////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

using System.ComponentModel.DataAnnotations;
using CSharp.Common;
using Kinetix.Modeling.Annotations;

namespace Models.CSharp.Securite.Profil.Models;

/// <summary>
/// Détail d'un profil en écriture.
/// </summary>
public partial record ProfilWrite
{
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
}
