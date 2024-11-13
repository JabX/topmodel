////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.profil;

import java.util.Arrays;

/**
 * Enumération des valeurs possibles de la classe TypeDroit.
 */
public enum TypeDroitEnum {
	/**
	 * Lecture.
	 */
	READ(TypeDroitCode.READ, "securite.profil.typeDroit.values.Read"), 

	/**
	 * Ecriture.
	 */
	WRITE(TypeDroitCode.WRITE, "securite.profil.typeDroit.values.Write"), 

	/**
	 * Administration.
	 */
	ADMIN(TypeDroitCode.ADMIN, "securite.profil.typeDroit.values.Admin"); 

	/**
	 * Code.
	 */
	private final TypeDroitCode code;

	/**
	 * Libelle.
	 */
	private final String libelle;

	/**
	 * Enum values constructor.
	 */
	private TypeDroitEnum(final TypeDroitCode code, final String libelle) {
		this.code = code;
		this.libelle = libelle;
	}

	/**
	 * Getter for code.
	 */
	public TypeDroitCode getCode() {
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
	public static TypeDroitEnum from(TypeDroitCode code) {
		return Arrays.stream(TypeDroitEnum.values()).filter(t -> t.getCode() == code).findFirst().orElseThrow();
	}
}
