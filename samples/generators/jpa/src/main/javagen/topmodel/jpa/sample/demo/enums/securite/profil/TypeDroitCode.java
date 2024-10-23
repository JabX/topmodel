////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.profil;

/**
 * Enumération des valeurs possibles de la propriété Code de la classe TypeDroit.
 */
public enum TypeDroitCode {
	/**
	 * Lecture.
	 */
	READ("securite.profil.typeDroit.values.Read"), 
	/**
	 * Ecriture.
	 */
	WRITE("securite.profil.typeDroit.values.Write"), 
	/**
	 * Administration.
	 */
	ADMIN("securite.profil.typeDroit.values.Admin"); 

	/**
	 * Libelle.
	 */
	private final String libelle;
	/**
	 * Enum constructor.
	 */
	TypeDroitCode(final String libelle ){
		 this.libelle = libelle;
	}

	/**
	 * Getter.
	 */
	public String getLibelle(){
		return this.libelle;
	}
}
