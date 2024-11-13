////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.profil;

import java.util.Objects;
import java.util.stream.Collectors;

import jakarta.annotation.Generated;

import topmodel.jpa.sample.demo.dtos.securite.profil.ProfilRead;
import topmodel.jpa.sample.demo.dtos.securite.profil.ProfilWrite;

@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class SecuriteProfilMappers {

	private SecuriteProfilMappers() {
		// private constructor to hide implicite public one
	}

	/**
	 * Map les champs des classes passées en paramètre dans l'objet target'.
	 * @param target Instance de 'ProfilRead' (ou null pour créer une nouvelle instance).
	 * @param profil Instance de 'Profil'.
	 *
	 * @return Une nouvelle instance de 'ProfilRead' ou bien l'instance passée en paramètres sur lesquels les champs sources ont été mappée.
	 */
	public static ProfilRead createProfilRead(Profil profil, ProfilRead target) {
		if (target == null) {
			target = new ProfilRead();
		}

		if (profil == null) {
			throw new IllegalArgumentException("profil cannot be null");
		}

		target.setId(profil.getId());
		target.setLibelle(profil.getLibelle());
		if (profil.getDroits() != null) {
			target.setDroits(profil.getDroits().stream().filter(Objects::nonNull).map(Droit::getCode).collect(Collectors.toList()));
		}

		target.setDateCreation(profil.getDateCreation());
		target.setDateModification(profil.getDateModification());
		return target;
	}

	/**
	 * Mappe 'Profil' vers 'Profil'.
	 * @param source Instance de 'Profil'.
	 * @param target Instance pré-existante de 'Profil'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une nouvelle instance de 'Profil' ou bien l'instance passée en paramètre dont les champs ont été surchargés.
	 */
	public static Profil toProfil(Profil source, Profil target) {
		if (source == null) {
			throw new IllegalArgumentException("source cannot be null");
		}

		if (target == null) {
			target = new Profil();
		}

		target.setLibelle(source.getLibelle());
		target.setDroits(source.getDroits());
		return target;
	}

	/**
	 * Mappe 'ProfilWrite' vers 'Profil'.
	 * @param source Instance de 'ProfilWrite'.
	 * @param target Instance pré-existante de 'Profil'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une nouvelle instance de 'Profil' ou bien l'instance passée en paramètre dont les champs ont été surchargés.
	 */
	public static Profil toProfil(ProfilWrite source, Profil target) {
		if (source == null) {
			throw new IllegalArgumentException("source cannot be null");
		}

		if (target == null) {
			target = new Profil();
		}

		target.setLibelle(source.getLibelle());
		target.setDroits(source.getDroits().stream().map(Droit::from).collect(Collectors.toList()));
		return target;
	}
}
