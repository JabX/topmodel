////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

package topmodel.jpa.sample.demo.converters.securite.profil;

import jakarta.persistence.AttributeConverter;

import topmodel.jpa.sample.demo.entities.securite.profil.Droit;

public class DroitConverter implements AttributeConverter<Droit, String> {

	@Override
	public String convertToDatabaseColumn(Droit item) {
		return item == null ? null : item.getCode().name();
	}

	@Override
	public Droit convertToEntityAttribute(String code) {
		return Droit.from(code);
	}
}
