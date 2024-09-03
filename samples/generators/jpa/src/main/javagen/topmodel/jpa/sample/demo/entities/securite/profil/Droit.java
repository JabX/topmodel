////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.profil;

import java.util.NoSuchElementException;

import jakarta.annotation.Generated;
import jakarta.persistence.Convert;

import topmodel.jpa.sample.demo.converters.securite.profil.TypeDroitConverter;
import topmodel.jpa.sample.demo.enums.securite.profil.DroitCode;

/**
 * Droits de l'application.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
public enum Droit {
	Create(DroitCode.CREATE),
	Delete(DroitCode.DELETE),
	Read(DroitCode.READ),
	Update(DroitCode.UPDATE);

	/**
	 * Code du droit.
	 */
	private DroitCode code;

	/**
	 * Libellé du droit.
	 */
	private String libelle;

	/**
	 * Type de profil pouvant faire l'action.
	 */
	@Convert(converter = TypeDroitConverter.class)
	private TypeDroit typeDroit;

	/**
	 * Enum constructor.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	private Droit(DroitCode code) {
		this.code = code;
		switch(code) {
			case CREATE :
				this.libelle = "securite.profil.droit.values.Create";
				this.typeDroit = TypeDroit.WRITE;
				break;
			case DELETE :
				this.libelle = "securite.profil.droit.values.Delete";
				this.typeDroit = TypeDroit.ADMIN;
				break;
			case READ :
				this.libelle = "securite.profil.droit.values.Read";
				this.typeDroit = TypeDroit.READ;
				break;
			case UPDATE :
				this.libelle = "securite.profil.droit.values.Update";
				this.typeDroit = TypeDroit.WRITE;
				break;
			}
	}

	public static Droit from(String code) {
		return from(DroitCode.valueOf(code));
	}

	public static Droit from(DroitCode code) {
		switch(code) {
			case CREATE:
				return Create;
			case DELETE:
				return Delete;
			case READ:
				return Read;
			case UPDATE:
				return Update;
			default:
				throw new NoSuchElementException(code + " value unrecognized");
		}
	}

	/**
	 * Getter for code.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Droit#code code}.
	 */
	public DroitCode getCode() {
		return this.code;
	}

	/**
	 * Getter for libelle.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Droit#libelle libelle}.
	 */
	public String getLibelle() {
		return this.libelle;
	}

	/**
	 * Getter for typeDroit.
	 *
	 * @return value of {@link topmodel.jpa.sample.demo.entities.securite.profil.Droit#typeDroit typeDroit}.
	 */
	public TypeDroit getTypeDroit() {
		return this.typeDroit;
	}

	/**
	 * Enumération des champs de la classe {@link topmodel.jpa.sample.demo.entities.securite.profil.Droit Droit}.
	 */
	public enum Fields  {
        CODE(DroitCode.class), //
        LIBELLE(String.class), //
        TYPE_DROIT(TypeDroit.class);

		private Class<?> type;

		private Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
