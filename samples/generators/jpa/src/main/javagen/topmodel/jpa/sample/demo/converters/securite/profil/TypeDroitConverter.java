////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.converters.securite.profil;

import jakarta.persistence.AttributeConverter;

import topmodel.jpa.sample.demo.entities.securite.profil.TypeDroit;

public class TypeDroitConverter implements AttributeConverter<TypeDroit, String> {

	@Override
	public String convertToDatabaseColumn(TypeDroit item) {
		return item == null ? null : item.getCode().name();
	}

	@Override
	public TypeDroit convertToEntityAttribute(String code) {
		return TypeDroit.from(code);
	}
}
