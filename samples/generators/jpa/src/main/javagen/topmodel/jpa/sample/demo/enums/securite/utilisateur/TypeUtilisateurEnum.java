////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.utilisateur;

import java.util.Arrays;

/**
 * Enumération des valeurs possibles de la classe TypeUtilisateur.
 */
public enum TypeUtilisateurEnum {
	/**
	 * Administrateur.
	 */
	ADMIN(TypeUtilisateurCode.ADMIN, "securite.utilisateur.typeUtilisateur.values.Admin"), 

	/**
	 * Gestionnaire.
	 */
	GESTIONNAIRE(TypeUtilisateurCode.GEST, "securite.utilisateur.typeUtilisateur.values.Gestionnaire"), 

	/**
	 * Client.
	 */
	CLIENT(TypeUtilisateurCode.CLIENT, "securite.utilisateur.typeUtilisateur.values.Client"); 

	/**
	 * Code.
	 */
	private final TypeUtilisateurCode code;

	/**
	 * Libelle.
	 */
	private final String libelle;

	/**
	 * Enum values constructor.
	 */
	private TypeUtilisateurEnum(final TypeUtilisateurCode code, final String libelle) {
		this.code = code;
		this.libelle = libelle;
	}

	/**
	 * Getter for code.
	 */
	public TypeUtilisateurCode getCode() {
		return this.code;
	}

	/**
	 * Getter for libelle.
	 */
	public String getLibelle() {
		return this.libelle;
	}


	/**
	 * Get Enum from pk.
	 */
	public static TypeUtilisateurEnum from(TypeUtilisateurCode code) {
		return Arrays.stream(TypeUtilisateurEnum.values()).filter(t -> t.getCode() == code).findFirst().orElseThrow();
	}
}
