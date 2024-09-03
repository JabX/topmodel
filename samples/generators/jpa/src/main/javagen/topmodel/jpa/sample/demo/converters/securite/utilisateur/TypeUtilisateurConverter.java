////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.converters.securite.utilisateur;

import jakarta.persistence.AttributeConverter;

import topmodel.jpa.sample.demo.entities.securite.utilisateur.TypeUtilisateur;

public class TypeUtilisateurConverter implements AttributeConverter<TypeUtilisateur, String> {

	@Override
	public String convertToDatabaseColumn(TypeUtilisateur item) {
		return item == null ? null : item.getCode().name();
	}

	@Override
	public TypeUtilisateur convertToEntityAttribute(String code) {
		return TypeUtilisateur.from(code);
	}
}
