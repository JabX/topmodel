////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.dtos.securite.profil;

import java.io.Serializable;
import java.time.LocalDateTime;
import java.util.List;

import jakarta.annotation.Generated;
import jakarta.validation.constraints.NotNull;

import topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurItem;
import topmodel.jpa.sample.demo.entities.securite.profil.Profil;
import topmodel.jpa.sample.demo.entities.securite.profil.SecuriteProfilMappers;
import topmodel.jpa.sample.demo.enums.securite.profil.DroitCode;

/**
 * Détail d'un profil en lecture.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class ProfilRead implements Serializable {
	/** Serial ID */
	private static final long serialVersionUID = 1L;

	/**
	 * Id technique.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#getId() Profil#getId()} 
	 */
	@NotNull
	private Integer id;

	/**
	 * Libellé du profil.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#getLibelle() Profil#getLibelle()} 
	 */
	@NotNull
	private String libelle;

	/**
	 * Liste des droits du profil.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#getDroits() Profil#getDroits()} 
	 */
	private List<DroitCode> droits;

	/**
	 * Date de création de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#getDateCreation() Profil#getDateCreation()} 
	 */
	@NotNull
	private LocalDateTime dateCreation;

	/**
	 * Date de modification de l'utilisateur.
	 * Alias of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#getDateModification() Profil#getDateModification()} 
	 */
	private LocalDateTime dateModification;

	/**
	 * Utilisateurs ayant ce profil.
	 */
	private List<UtilisateurItem> utilisateurs;

	/**
	 * No arg constructor.
	 */
	public ProfilRead() {
		// No arg constructor
	}

	/**
	 * Crée une nouvelle instance de 'ProfilRead'.
	 * @param profil Instance de 'Profil'.
	 *
	 * @return Une nouvelle instance de 'ProfilRead'.
	 */
	public ProfilRead(Profil profil) {
		SecuriteProfilMappers.createProfilRead(profil, this);
	}

	/**
	 * Getter for id.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#id id}.
	 */
	public Integer getId() {
		return this.id;
	}

	/**
	 * Getter for libelle.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#libelle libelle}.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Getter for droits.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#droits droits}.
	 */
	public List<DroitCode> getDroits() {
		return this.droits;
	}

	/**
	 * Getter for dateCreation.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#dateCreation dateCreation}.
	 */
	public LocalDateTime getDateCreation() {
		return this.dateCreation;
	}

	/**
	 * Getter for dateModification.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#dateModification dateModification}.
	 */
	public LocalDateTime getDateModification() {
		return this.dateModification;
	}

	/**
	 * Getter for utilisateurs.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#utilisateurs utilisateurs}.
	 */
	public List<UtilisateurItem> getUtilisateurs() {
		return this.utilisateurs;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#id id}.
	 * @param id value to set
	 */
	public void setId(Integer id) {
		this.id = id;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#libelle libelle}.
	 * @param libelle value to set
	 */
	public void setLibelle(String libelle) {
		this.libelle = libelle;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#droits droits}.
	 * @param droits value to set
	 */
	public void setDroits(List<DroitCode> droits) {
		this.droits = droits;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#dateCreation dateCreation}.
	 * @param dateCreation value to set
	 */
	public void setDateCreation(LocalDateTime dateCreation) {
		this.dateCreation = dateCreation;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#dateModification dateModification}.
	 * @param dateModification value to set
	 */
	public void setDateModification(LocalDateTime dateModification) {
		this.dateModification = dateModification;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead#utilisateurs utilisateurs}.
	 * @param utilisateurs value to set
	 */
	public void setUtilisateurs(List<UtilisateurItem> utilisateurs) {
		this.utilisateurs = utilisateurs;
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead ProfilRead}.
	 */
	public enum Fields  {
        ID(Integer.class), //
        LIBELLE(String.class), //
        DROITS(List.class), //
        DATE_CREATION(LocalDateTime.class), //
        DATE_MODIFICATION(LocalDateTime.class), //
        UTILISATEURS(List.class);

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
