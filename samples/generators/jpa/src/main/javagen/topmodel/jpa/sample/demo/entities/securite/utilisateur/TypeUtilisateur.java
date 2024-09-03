////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.utilisateur;

import java.util.NoSuchElementException;

import jakarta.annotation.Generated;

import topmodel.jpa.sample.demo.enums.securite.utilisateur.TypeUtilisateurCode;

/**
 * Type d'utilisateur.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public enum TypeUtilisateur {
	Admin(TypeUtilisateurCode.ADMIN),
	Client(TypeUtilisateurCode.CLIENT),
	Gestionnaire(TypeUtilisateurCode.GEST);

	/**
	 * Code du type d'utilisateur.
	 */
	private TypeUtilisateurCode code;

	/**
	 * Libellé du type d'utilisateur.
	 */
	private String libelle;

	/**
	 * Enum constructor.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	private TypeUtilisateur(TypeUtilisateurCode code) {
		this.code = code;
		switch(code) {
			case ADMIN :
				this.libelle = "securite.utilisateur.typeUtilisateur.values.Admin";
				break;
			case CLIENT :
				this.libelle = "securite.utilisateur.typeUtilisateur.values.Client";
				break;
			case GEST :
				this.libelle = "securite.utilisateur.typeUtilisateur.values.Gestionnaire";
				break;
			}
	}

	public static TypeUtilisateur from(String code) {
		return from(TypeUtilisateurCode.valueOf(code));
	}

	public static TypeUtilisateur from(TypeUtilisateurCode code) {
		switch(code) {
			case ADMIN:
				return Admin;
			case CLIENT:
				return Client;
			case GEST:
				return Gestionnaire;
			default:
				throw new NoSuchElementException(code + " value unrecognized");
		}
	}

	/**
	 * Getter for code.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.TypeUtilisateur#code code}.
	 */
	public TypeUtilisateurCode getCode() {
		return this.code;
	}

	/**
	 * Getter for libelle.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.TypeUtilisateur#libelle libelle}.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.entities.securite.utilisateur.TypeUtilisateur TypeUtilisateur}.
	 */
	public enum Fields  {
        CODE(TypeUtilisateurCode.class), //
        LIBELLE(String.class);

		private Class<?> type;

		private Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
