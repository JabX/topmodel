////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.utilisateur;

import jakarta.annotation.Generated;

import topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead;
import topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite;

@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public class SecuriteUtilisateurMappers {

	private SecuriteUtilisateurMappers() {
		// private constructor to hide implicite public one
	}

	/**
	 * Map les champs des classes passées en paramètre dans l'objet target'.
	 * @param target Instance de 'UtilisateurRead' (ou null pour créer une nouvelle instance).
	 * @param utilisateur Instance de 'Utilisateur'.
	 *
	 * @return Une nouvelle instance de 'UtilisateurRead' ou bien l'instance passée en paramètres sur lesquels les champs sources ont été mappée.
	 */
	public static UtilisateurRead createUtilisateurRead(Utilisateur utilisateur, UtilisateurRead target) {
		if (target == null) {
			target = new UtilisateurRead();
		}

		if (utilisateur == null) {
			throw new IllegalArgumentException("utilisateur cannot be null");
		}

		target.setId(utilisateur.getId());
		target.setNom(utilisateur.getNom());
		target.setPrenom(utilisateur.getPrenom());
		target.setEmail(utilisateur.getEmail());
		target.setDateNaissance(utilisateur.getDateNaissance());
		target.setAdresse(utilisateur.getAdresse());
		target.setActif(utilisateur.getActif());
		if (utilisateur.getProfil() != null) {
			target.setProfilId(utilisateur.getProfil().getId());
		}

		if (utilisateur.getTypeUtilisateur() != null) {
			target.setTypeUtilisateurCode(utilisateur.getTypeUtilisateur().getCode());
		}

		target.setDateCreation(utilisateur.getDateCreation());
		target.setDateModification(utilisateur.getDateModification());
		return target;
	}

	/**
	 * Mappe 'UtilisateurWrite' vers 'Utilisateur'.
	 * @param source Instance de 'UtilisateurWrite'.
	 * @param target Instance pré-existante de 'Utilisateur'. Une nouvelle instance sera créée si non spécifié.
	 *
	 * @return Une nouvelle instance de 'Utilisateur' ou bien l'instance passée en paramètre dont les champs ont été surchargés.
	 */
	public static Utilisateur toUtilisateur(UtilisateurWrite source, Utilisateur target) {
		if (source == null) {
			throw new IllegalArgumentException("source cannot be null");
		}

		if (target == null) {
			target = new Utilisateur();
		}

		target.setNom(source.getNom());
		target.setPrenom(source.getPrenom());
		target.setEmail(source.getEmail());
		target.setDateNaissance(source.getDateNaissance());
		target.setAdresse(source.getAdresse());
		target.setActif(source.getActif());
		if (source.getTypeUtilisateurCode() != null) {
			target.setTypeUtilisateur(TypeUtilisateur.from(source.getTypeUtilisateurCode()));
		}

		return target;
	}
}
