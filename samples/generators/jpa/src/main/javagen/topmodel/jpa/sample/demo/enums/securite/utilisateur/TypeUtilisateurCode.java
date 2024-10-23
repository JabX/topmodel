////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.enums.securite.utilisateur;

/**
 * Enumération des valeurs possibles de la propriété Code de la classe TypeUtilisateur.
 */
public enum TypeUtilisateurCode {
	/**
	 * Administrateur.
	 */
	ADMIN("securite.utilisateur.typeUtilisateur.values.Admin"), 
	/**
	 * Gestionnaire.
	 */
	GEST("securite.utilisateur.typeUtilisateur.values.Gestionnaire"), 
	/**
	 * Client.
	 */
	CLIENT("securite.utilisateur.typeUtilisateur.values.Client"); 

	/**
	 * Libelle.
	 */
	private final String libelle;
	/**
	 * Enum constructor.
	 */
	TypeUtilisateurCode(final String libelle ){
		 this.libelle = libelle;
	}

	/**
	 * Getter.
	 */
	public String getLibelle(){
		return this.libelle;
	}
}
