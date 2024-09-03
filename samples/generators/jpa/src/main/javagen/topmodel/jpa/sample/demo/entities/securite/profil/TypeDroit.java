////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.profil;

import java.util.NoSuchElementException;

import jakarta.annotation.Generated;

import topmodel.jpa.sample.demo.enums.securite.profil.TypeDroitCode;

/**
 * Type de droit.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public enum TypeDroit {
	Admin(TypeDroitCode.ADMIN),
	Read(TypeDroitCode.READ),
	Write(TypeDroitCode.WRITE);

	/**
	 * Code du type de droit.
	 */
	private TypeDroitCode code;

	/**
	 * Libellé du type de droit.
	 */
	private String libelle;

	/**
	 * Enum constructor.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	private TypeDroit(TypeDroitCode code) {
		this.code = code;
		switch(code) {
			case ADMIN :
				this.libelle = "securite.profil.typeDroit.values.Admin";
				break;
			case READ :
				this.libelle = "securite.profil.typeDroit.values.Read";
				break;
			case WRITE :
				this.libelle = "securite.profil.typeDroit.values.Write";
				break;
			}
	}

	public static TypeDroit from(String code) {
		return from(TypeDroitCode.valueOf(code));
	}

	public static TypeDroit from(TypeDroitCode code) {
		switch(code) {
			case ADMIN:
				return Admin;
			case READ:
				return Read;
			case WRITE:
				return Write;
			default:
				throw new NoSuchElementException(code + " value unrecognized");
		}
	}

	/**
	 * Getter for code.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.TypeDroit#code code}.
	 */
	public TypeDroitCode getCode() {
		return this.code;
	}

	/**
	 * Getter for libelle.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.TypeDroit#libelle libelle}.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.entities.securite.profil.TypeDroit TypeDroit}.
	 */
	public enum Fields  {
        CODE(TypeDroitCode.class), //
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
