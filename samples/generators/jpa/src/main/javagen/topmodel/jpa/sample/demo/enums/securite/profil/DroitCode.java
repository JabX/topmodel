////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.profil;

/**
 * Enumération des valeurs possibles de la propriété Code de la classe Droit.
 */
public enum DroitCode {
	/**
	 * Création.
	 */
	CREATE("securite.profil.droit.values.Create", TypeDroitCode.WRITE), 
	/**
	 * Lecture.
	 */
	READ("securite.profil.droit.values.Read", TypeDroitCode.READ), 
	/**
	 * Mise à jour.
	 */
	UPDATE("securite.profil.droit.values.Update", TypeDroitCode.WRITE), 
	/**
	 * Suppression.
	 */
	DELETE("securite.profil.droit.values.Delete", TypeDroitCode.ADMIN); 

	/**
	 * Libelle.
	 */
	private final String libelle;

	/**
	 * TypeDroit.
	 */
	private final TypeDroitCode typeDroit;
	/**
	 * Enum constructor.
	 */
	DroitCode(final String libelle ,final TypeDroitCode typeDroit ){
		 this.libelle = libelle;
		 this.typeDroit = typeDroit;
	}

	/**
	 * Getter.
	 */
	public String getLibelle(){
		return this.libelle;
	}

	/**
	 * Getter.
	 */
	public TypeDroitCode getTypeDroit(){
		return this.typeDroit;
	}
}
