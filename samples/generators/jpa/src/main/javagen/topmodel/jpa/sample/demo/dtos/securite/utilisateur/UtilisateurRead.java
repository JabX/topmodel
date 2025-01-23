////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.dtos.securite.utilisateur;

import java.io.Serializable;
import java.time.LocalDate;
import java.time.LocalDateTime;

import jakarta.annotation.Generated;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotNull;

import topmodel.jpa.sample.demo.entities.securite.utilisateur.SecuriteUtilisateurMappers;
import topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur;
import topmodel.jpa.sample.demo.enums.securite.utilisateur.TypeUtilisateurCode;

/**
 * Détail d'un utilisateur en lecture.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class UtilisateurRead implements Serializable {
	/** Serial ID */
	private static final long serialVersionUID = 1L;

	/**
	 * Id de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getId() Utilisateur#getId()} 
	 */
	@NotNull
	private Integer id;

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
	@Email
	@NotNull
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
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getProfil() Utilisateur#getProfil()} 
	 */
	@NotNull
	private Integer profilId;

	/**
	 * Type d'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getTypeUtilisateur() Utilisateur#getTypeUtilisateur()} 
	 */
	@NotNull
	private TypeUtilisateurCode typeUtilisateurCode = TypeUtilisateurCode.GEST;

	/**
	 * Date de création de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getDateCreation() Utilisateur#getDateCreation()} 
	 */
	@NotNull
	private LocalDateTime dateCreation;

	/**
	 * Date de modification de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur#getDateModification() Utilisateur#getDateModification()} 
	 */
	private LocalDateTime dateModification;

	/**
	 * No arg constructor.
	 */
	public UtilisateurRead() {
		// No arg constructor
	}

	/**
	 * Crée une nouvelle instance de 'UtilisateurRead'.
	 * @param utilisateur Instance de 'Utilisateur'.
	 *
	 * @return Une nouvelle instance de 'UtilisateurRead'.
	 */
	public UtilisateurRead(Utilisateur utilisateur) {
		SecuriteUtilisateurMappers.createUtilisateurRead(utilisateur, this);
	}

	/**
	 * Getter for id.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#id id}.
	 */
	public Integer getId() {
		return this.id;
	}

	/**
	 * Getter for nom.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#nom nom}.
	 */
	public String getNom() {
		return this.nom;
	}

	/**
	 * Getter for prenom.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#prenom prenom}.
	 */
	public String getPrenom() {
		return this.prenom;
	}

	/**
	 * Getter for email.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#email email}.
	 */
	public String getEmail() {
		return this.email;
	}

	/**
	 * Getter for dateNaissance.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#dateNaissance dateNaissance}.
	 */
	public LocalDate getDateNaissance() {
		return this.dateNaissance;
	}

	/**
	 * Getter for adresse.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#adresse adresse}.
	 */
	public String getAdresse() {
		return this.adresse;
	}

	/**
	 * Getter for actif.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#actif actif}.
	 */
	public Boolean getActif() {
		return this.actif;
	}

	/**
	 * Getter for profilId.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#profilId profilId}.
	 */
	public Integer getProfilId() {
		return this.profilId;
	}

	/**
	 * Getter for typeUtilisateurCode.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#typeUtilisateurCode typeUtilisateurCode}.
	 */
	public TypeUtilisateurCode getTypeUtilisateurCode() {
		return this.typeUtilisateurCode;
	}

	/**
	 * Getter for dateCreation.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#dateCreation dateCreation}.
	 */
	public LocalDateTime getDateCreation() {
		return this.dateCreation;
	}

	/**
	 * Getter for dateModification.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#dateModification dateModification}.
	 */
	public LocalDateTime getDateModification() {
		return this.dateModification;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#id id}.
	 * @param id value to set.
	 */
	public void setId(Integer id) {
		this.id = id;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#nom nom}.
	 * @param nom value to set.
	 */
	public void setNom(String nom) {
		this.nom = nom;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#prenom prenom}.
	 * @param prenom value to set.
	 */
	public void setPrenom(String prenom) {
		this.prenom = prenom;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#email email}.
	 * @param email value to set.
	 */
	public void setEmail(String email) {
		this.email = email;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#dateNaissance dateNaissance}.
	 * @param dateNaissance value to set.
	 */
	public void setDateNaissance(LocalDate dateNaissance) {
		this.dateNaissance = dateNaissance;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#adresse adresse}.
	 * @param adresse value to set.
	 */
	public void setAdresse(String adresse) {
		this.adresse = adresse;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#actif actif}.
	 * @param actif value to set.
	 */
	public void setActif(Boolean actif) {
		this.actif = actif;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#profilId profilId}.
	 * @param profilId value to set.
	 */
	public void setProfilId(Integer profilId) {
		this.profilId = profilId;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#typeUtilisateurCode typeUtilisateurCode}.
	 * @param typeUtilisateurCode value to set.
	 */
	public void setTypeUtilisateurCode(TypeUtilisateurCode typeUtilisateurCode) {
		this.typeUtilisateurCode = typeUtilisateurCode;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#dateCreation dateCreation}.
	 * @param dateCreation value to set.
	 */
	public void setDateCreation(LocalDateTime dateCreation) {
		this.dateCreation = dateCreation;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead#dateModification dateModification}.
	 * @param dateModification value to set.
	 */
	public void setDateModification(LocalDateTime dateModification) {
		this.dateModification = dateModification;
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead UtilisateurRead}.
	 */
	public enum Fields  {
        ID(Integer.class), //
        NOM(String.class), //
        PRENOM(String.class), //
        EMAIL(String.class), //
        DATE_NAISSANCE(LocalDate.class), //
        ADRESSE(String.class), //
        ACTIF(Boolean.class), //
        PROFIL_ID(Integer.class), //
        TYPE_UTILISATEUR_CODE(TypeUtilisateurCode.class), //
        DATE_CREATION(LocalDateTime.class), //
        DATE_MODIFICATION(LocalDateTime.class);

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
