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
import jakarta.persistence.FetchType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Table;
import jakarta.persistence.Transient;

import topmodel.jpa.sample.demo.enums.securite.profil.DroitCode;

/**
 * Droits de l'application.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
@Entity
@Table(name = "DROIT")
@Immutable
@Cache(usage = CacheConcurrencyStrategy.READ_ONLY)
public class Droit {

	@Transient
	public static final Droit CREATE = new Droit(DroitCode.CREATE);
	@Transient
	public static final Droit DELETE = new Droit(DroitCode.DELETE);
	@Transient
	public static final Droit READ = new Droit(DroitCode.READ);
	@Transient
	public static final Droit UPDATE = new Droit(DroitCode.UPDATE);

	/**
	 * Code du droit.
	 */
	@Id
	@Column(name = "DRO_CODE", nullable = false, length = 10, columnDefinition = "varchar")
	@Enumerated(EnumType.STRING)
	private DroitCode code;

	/**
	 * Libellé du droit.
	 */
	@Column(name = "DRO_LIBELLE", nullable = false, length = 100, columnDefinition = "varchar")
	private String libelle;

	/**
	 * Type de profil pouvant faire l'action.
	 */
	@ManyToOne(fetch = FetchType.LAZY, optional = false, targetEntity = TypeDroit.class)
	@JoinColumn(name = "TDR_CODE", referencedColumnName = "TDR_CODE")
	private TypeDroit typeDroit;

	/**
	 * No arg constructor.
	 */
	public Droit() {
		// No arg constructor
	}

	/**
	 * Enum constructor.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	public Droit(DroitCode code) {
		this.code = code;
		this.libelle = code.getLibelle();
		this.typeDroit = code.getTypeDroitCode() != null ? new TypeDroit(code.getTypeDroitCode()) : null;
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

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
