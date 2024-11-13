////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.profil;

import java.util.Arrays;

/**
 * Enumération des valeurs possibles de la classe Droit.
 */
public enum DroitEnum {
	/**
	 * Création.
	 */
	CREATE(DroitCode.CREATE, "securite.profil.droit.values.Create", TypeDroitEnum.WRITE), 

	/**
	 * Lecture.
	 */
	READ(DroitCode.READ, "securite.profil.droit.values.Read", TypeDroitEnum.READ), 

	/**
	 * Mise à jour.
	 */
	UPDATE(DroitCode.UPDATE, "securite.profil.droit.values.Update", TypeDroitEnum.WRITE), 

	/**
	 * Suppression.
	 */
	DELETE(DroitCode.DELETE, "securite.profil.droit.values.Delete", TypeDroitEnum.ADMIN); 

	/**
	 * Code.
	 */
	private final DroitCode code;

	/**
	 * Libelle.
	 */
	private final String libelle;

	/**
	 * TypeDroit.
	 */
	private final TypeDroitEnum typeDroitEnum;

	/**
	 * Enum values constructor.
	 */
	private DroitEnum(final DroitCode code, final String libelle, final TypeDroitEnum typeDroitEnum) {
		this.code = code;
		this.libelle = libelle;
		this.typeDroitEnum = typeDroitEnum;
	}

	/**
	 * Getter for code.
	 */
	public DroitCode getCode() {
		return this.code;
	}

	/**
	 * Getter for libelle.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Getter for typeDroit.
	 */
	public TypeDroitEnum getTypeDroitEnum() {
		return this.typeDroitEnum;
	}


	/**
	 * Get Enum from pk.
	 */
	public static DroitEnum from(DroitCode code) {
		return Arrays.stream(DroitEnum.values()).filter(t -> t.getCode() == code).findFirst().orElseThrow();
	}
}
