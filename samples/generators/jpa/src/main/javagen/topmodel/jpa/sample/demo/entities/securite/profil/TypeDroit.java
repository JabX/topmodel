////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.profil;

import org.hibernate.annotations.Cache;
import org.hibernate.annotations.CacheConcurrencyStrategy;
import org.hibernate.annotations.Immutable;

import jakarta.annotation.Generated;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Enumerated;
import jakarta.persistence.EnumType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import topmodel.jpa.sample.demo.enums.securite.profil.TypeDroitCode;

/**
 * Type de droit.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
@Entity
@Table(name = "TYPE_DROIT")
@Immutable
@Cache(usage = CacheConcurrencyStrategy.READ_ONLY)
public class TypeDroit {

	@Transient
	public static final TypeDroit ADMIN = new TypeDroit(TypeDroitCode.ADMIN);
	@Transient
	public static final TypeDroit READ = new TypeDroit(TypeDroitCode.READ);
	@Transient
	public static final TypeDroit WRITE = new TypeDroit(TypeDroitCode.WRITE);

	/**
	 * Code du type de droit.
	 */
	@Id
	@Column(name = "TDR_CODE", nullable = false, length = 10, columnDefinition = "varchar")
	@Enumerated(EnumType.STRING)
	private TypeDroitCode code;

	/**
	 * Libellé du type de droit.
	 */
	@Column(name = "TDR_LIBELLE", nullable = false, length = 100, columnDefinition = "varchar")
	private String libelle;

	/**
	 * No arg constructor.
	 */
	public TypeDroit() {
		// No arg constructor
	}

	/**
	 * Enum constructor.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	public TypeDroit(TypeDroitCode code) {
		this.code = code;
		this.libelle = code.getLibelle();
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

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
