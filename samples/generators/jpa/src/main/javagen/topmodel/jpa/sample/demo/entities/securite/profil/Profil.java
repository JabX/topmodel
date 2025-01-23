////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.profil;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

import org.springframework.data.annotation.CreatedDate;
import org.springframework.data.annotation.LastModifiedDate;
import org.springframework.data.jpa.domain.support.AuditingEntityListener;

import jakarta.annotation.Generated;
import jakarta.persistence.CascadeType;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EntityListeners;
import jakarta.persistence.FetchType;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.JoinTable;
import jakarta.persistence.ManyToMany;
import jakarta.persistence.OneToMany;
import jakarta.persistence.OrderBy;
import jakarta.persistence.Table;

import topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur;

/**
 * Profil des utilisateurs.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
@EntityListeners(AuditingEntityListener.class)
@Entity
@Table(name = "PROFIL")
public class Profil {

	/**
	 * Id technique.
	 */
	@GeneratedValue(strategy = GenerationType.IDENTITY)
	@Id
	@Column(name = "PRO_ID", nullable = false, columnDefinition = "int")
	private Integer id;

	/**
	 * Libellé du profil.
	 */
	@Column(name = "PRO_LIBELLE", nullable = false, length = 100, columnDefinition = "varchar")
	private String libelle;

	/**
	 * Liste des droits du profil.
	 */
	@ManyToMany(fetch = FetchType.LAZY)
	@JoinTable(name = "PROFIL_DROIT", joinColumns = @JoinColumn(name = "PRO_ID"), inverseJoinColumns = @JoinColumn(name = "DRO_CODE"))
	@OrderBy("code ASC")
	private List<Droit> droits;

	/**
	 * Date de création de l'utilisateur.
	 */
	@CreatedDate
	@Column(name = "PRO_DATE_CREATION", nullable = false, columnDefinition = "date")
	private LocalDateTime dateCreation = LocalDateTime.now();

	/**
	 * Date de modification de l'utilisateur.
	 */
	@LastModifiedDate
	@Column(name = "PRO_DATE_MODIFICATION", columnDefinition = "date")
	private LocalDateTime dateModification = LocalDateTime.now();

	/**
	 * Association réciproque de Utilisateur.ProfilId.
	 */
	@OneToMany(cascade = {CascadeType.PERSIST, CascadeType.MERGE}, fetch = FetchType.LAZY, mappedBy = "profil")
	private Utilisateur utilisateurs;

	/**
	 * Getter for id.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#id id}.
	 */
	public Integer getId() {
		return this.id;
	}

	/**
	 * Getter for libelle.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#libelle libelle}.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Getter for droits.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#droits droits}.
	 */
	public List<Droit> getDroits() {
		if(this.droits == null) {
			this.droits = new ArrayList<>();
		}
		return this.droits;
	}

	/**
	 * Getter for dateCreation.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#dateCreation dateCreation}.
	 */
	public LocalDateTime getDateCreation() {
		return this.dateCreation;
	}

	/**
	 * Getter for dateModification.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#dateModification dateModification}.
	 */
	public LocalDateTime getDateModification() {
		return this.dateModification;
	}

	/**
	 * Getter for utilisateurs.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#utilisateurs utilisateurs}.
	 */
	public Utilisateur getUtilisateurs() {
		return this.utilisateurs;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#id id}.
	 * @param id value to set.
	 */
	public void setId(Integer id) {
		this.id = id;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#libelle libelle}.
	 * @param libelle value to set.
	 */
	public void setLibelle(String libelle) {
		this.libelle = libelle;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#droits droits}.
	 * @param droits value to set.
	 */
	public void setDroits(List<Droit> droits) {
		this.droits = droits;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#dateCreation dateCreation}.
	 * @param dateCreation value to set.
	 */
	public void setDateCreation(LocalDateTime dateCreation) {
		this.dateCreation = dateCreation;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#dateModification dateModification}.
	 * @param dateModification value to set.
	 */
	public void setDateModification(LocalDateTime dateModification) {
		this.dateModification = dateModification;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil#utilisateurs utilisateurs}.
	 * @param utilisateurs value to set.
	 */
	public void setUtilisateurs(Utilisateur utilisateurs) {
		this.utilisateurs = utilisateurs;
	}

	/**
	 * Mappe 'Profil' vers 'Profil'.
	 * @param target Instance pré-existante de 'Profil'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une instance de 'Profil'.
	 */
	public Profil toProfil(Profil target) {
		return SecuriteProfilMappers.toProfil(this, target);
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.entities.securite.profil.Profil Profil}.
	 */
	public enum Fields  {
        ID(Integer.class), //
        LIBELLE(String.class), //
        DROITS(List.class), //
        DATE_CREATION(LocalDateTime.class), //
        DATE_MODIFICATION(LocalDateTime.class), //
        UTILISATEURS(Utilisateur.class);

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
