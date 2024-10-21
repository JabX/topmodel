////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.dtos.securite.profil;

import java.io.Serializable;
import java.util.List;

import jakarta.annotation.Generated;
import jakarta.validation.constraints.NotNull;

import topmodel.jpa.sample.demo.entities.securite.profil.Profil;
import topmodel.jpa.sample.demo.entities.securite.profil.SecuriteProfilMappers;
import topmodel.jpa.sample.demo.enums.securite.profil.DroitCode;

/**
 * Détail d'un profil en écriture.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class ProfilWrite implements Serializable {
	/** Serial ID */
	private static final long serialVersionUID = 1L;

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
	 * Getter for libelle.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilWrite#libelle libelle}.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Getter for droits.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilWrite#droits droits}.
	 */
	public List<DroitCode> getDroits() {
		return this.droits;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilWrite#libelle libelle}.
	 * @param libelle value to set
	 */
	public void setLibelle(String libelle) {
		this.libelle = libelle;
	}

	/**
	 * Set the value of {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilWrite#droits droits}.
	 * @param droits value to set
	 */
	public void setDroits(List<DroitCode> droits) {
		this.droits = droits;
	}

	/**
	 * Mappe 'ProfilWrite' vers 'Profil'.
	 * @param target Instance pré-existante de 'Profil'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une instance de 'Profil'.
	 */
	public Profil toProfil(Profil target) {
		return SecuriteProfilMappers.toProfil(this, target);
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.dtos.securite.profil.ProfilWrite ProfilWrite}.
	 */
	public enum Fields  {
        LIBELLE(String.class), //
        DROITS(List.class);

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
