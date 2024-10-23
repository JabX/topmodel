////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.dtos.securite.utilisateur;

import java.io.Serializable;
import java.time.LocalDate;

import jakarta.annotation.Generated;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotNull;

import topmodel.jpa.sample.demo.entities.securite.utilisateur.SecuriteUtilisateurMappers;
import topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur;
import topmodel.jpa.sample.demo.enums.securite.utilisateur.TypeUtilisateurCode;

/**
 * Détail d'un utilisateur en écriture.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class UtilisateurWrite implements Serializable {
	/** Serial ID */
	private static final long serialVersionUID = 1L;

	/**
	 * Nom de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getNom() Utilisateur#getNom()} 
	 */
	@NotNull
	private String nom;

	/**
	 * Nom de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getPrenom() Utilisateur#getPrenom()} 
	 */
	@NotNull
	private String prenom;

	/**
	 * Email de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getEmail() Utilisateur#getEmail()} 
	 */
	@NotNull
	@Email
	private String email;

	/**
	 * Age de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getDateNaissance() Utilisateur#getDateNaissance()} 
	 */
	private LocalDate dateNaissance;

	/**
	 * Adresse de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getAdresse() Utilisateur#getAdresse()} 
	 */
	private String adresse;

	/**
	 * Si l'utilisateur est actif.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getActif() Utilisateur#getActif()} 
	 */
	@NotNull
	private Boolean actif = true;

	/**
	 * Profil de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getProfilId() Utilisateur#getProfilId()} 
	 */
	@NotNull
	private Integer profilId;

	/**
	 * Type d'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getTypeUtilisateurCode() Utilisateur#getTypeUtilisateurCode()} 
	 */
	@NotNull
	private TypeUtilisateurCode typeUtilisateurCode = TypeUtilisateurCode.GEST;

	/**
	 * Getter for nom.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#nom nom}.
	 */
	public String getNom() {
		return this.nom;
	}

	/**
	 * Getter for prenom.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#prenom prenom}.
	 */
	public String getPrenom() {
		return this.prenom;
	}

	/**
	 * Getter for email.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#email email}.
	 */
	public String getEmail() {
		return this.email;
	}

	/**
	 * Getter for dateNaissance.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#dateNaissance dateNaissance}.
	 */
	public LocalDate getDateNaissance() {
		return this.dateNaissance;
	}

	/**
	 * Getter for adresse.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#adresse adresse}.
	 */
	public String getAdresse() {
		return this.adresse;
	}

	/**
	 * Getter for actif.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#actif actif}.
	 */
	public Boolean getActif() {
		return this.actif;
	}

	/**
	 * Getter for profilId.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#profilId profilId}.
	 */
	public Integer getProfilId() {
		return this.profilId;
	}

	/**
	 * Getter for typeUtilisateurCode.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#typeUtilisateurCode typeUtilisateurCode}.
	 */
	public TypeUtilisateurCode getTypeUtilisateurCode() {
		return this.typeUtilisateurCode;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#nom nom}.
	 * @param nom value to set
	 */
	public void setNom(String nom) {
		this.nom = nom;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#prenom prenom}.
	 * @param prenom value to set
	 */
	public void setPrenom(String prenom) {
		this.prenom = prenom;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#email email}.
	 * @param email value to set
	 */
	public void setEmail(String email) {
		this.email = email;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#dateNaissance dateNaissance}.
	 * @param dateNaissance value to set
	 */
	public void setDateNaissance(LocalDate dateNaissance) {
		this.dateNaissance = dateNaissance;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#adresse adresse}.
	 * @param adresse value to set
	 */
	public void setAdresse(String adresse) {
		this.adresse = adresse;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#actif actif}.
	 * @param actif value to set
	 */
	public void setActif(Boolean actif) {
		this.actif = actif;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#profilId profilId}.
	 * @param profilId value to set
	 */
	public void setProfilId(Integer profilId) {
		this.profilId = profilId;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite#typeUtilisateurCode typeUtilisateurCode}.
	 * @param typeUtilisateurCode value to set
	 */
	public void setTypeUtilisateurCode(TypeUtilisateurCode typeUtilisateurCode) {
		this.typeUtilisateurCode = typeUtilisateurCode;
	}

	/**
	 * Mappe 'UtilisateurWrite' vers 'Utilisateur'.
	 * @param target Instance pré-existante de 'Utilisateur'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une instance de 'Utilisateur'.
	 */
	public Utilisateur toUtilisateur(Utilisateur target) {
		return SecuriteUtilisateurMappers.toUtilisateur(this, target);
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite UtilisateurWrite}.
	 */
	public enum Fields  {
        NOM(String.class), //
        PRENOM(String.class), //
        EMAIL(String.class), //
        DATE_NAISSANCE(LocalDate.class), //
        ADRESSE(String.class), //
        ACTIF(Boolean.class), //
        PROFIL_ID(Integer.class), //
        TYPE_UTILISATEUR_CODE(TypeUtilisateurCode.class);

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
