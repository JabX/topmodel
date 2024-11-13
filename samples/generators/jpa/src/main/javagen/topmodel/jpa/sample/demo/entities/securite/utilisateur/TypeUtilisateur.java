////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.entities.securite.utilisateur;

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

import topmodel.jpa.sample.demo.enums.securite.utilisateur.TypeUtilisateurCode;
import topmodel.jpa.sample.demo.enums.securite.utilisateur.TypeUtilisateurEnum;

/**
 * Type d'utilisateur.
 */
@Generated("TopModel : https://github.com/klee-contrib/topmodel")
@Entity
@Table(name = "TYPE_UTILISATEUR")
@Immutable
@Cache(usage = CacheConcurrencyStrategy.READ_ONLY)
public class TypeUtilisateur {

	@Transient
	public static final TypeUtilisateur ADMIN = new TypeUtilisateur(TypeUtilisateurEnum.ADMIN);
	@Transient
	public static final TypeUtilisateur CLIENT = new TypeUtilisateur(TypeUtilisateurEnum.CLIENT);
	@Transient
	public static final TypeUtilisateur GESTIONNAIRE = new TypeUtilisateur(TypeUtilisateurEnum.GESTIONNAIRE);

	/**
	 * Code du type d'utilisateur.
	 */
	@Id
	@Column(name = "TUT_CODE", nullable = false, length = 10, columnDefinition = "varchar")
	@Enumerated(EnumType.STRING)
	private TypeUtilisateurCode code;

	/**
	 * Libellé du type d'utilisateur.
	 */
	@Column(name = "TUT_LIBELLE", nullable = false, length = 100, columnDefinition = "varchar")
	private String libelle;

	/**
	 * No arg constructor.
	 */
	public TypeUtilisateur() {
		// No arg constructor
	}

	/**
	 * Enum code finder.
	 * @param code Code dont on veut obtenir l'instance.
	 */
	public static TypeUtilisateur from(TypeUtilisateurCode code) {
		return switch (code) {
			case ADMIN -> ADMIN;
			case CLIENT -> CLIENT;
			case GESTIONNAIRE -> GESTIONNAIRE;
		};
	}

	/**
	 * Enum constructor.
	 * @param typeUtilisateurEnum Enum de valeur dont on veut obtenir l'entité.
	 */
	public TypeUtilisateur(TypeUtilisateurEnum typeUtilisateurEnum) {
		this.code = typeUtilisateurEnum.getCode();
		this.libelle = typeUtilisateurEnum.getLibelle();
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

		private final Class<?> type;

		Fields(Class<?> type) {
			this.type = type;
		}

		public Class<?> getType() {
			return this.type;
		}
	}
}
