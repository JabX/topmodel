import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

import java.time.LocalDate;
import java.util.Arrays;

import org.junit.jupiter.api.Test;

import topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurRead;
import topmodel.jpa.sample.demo.dtos.securite.utilisateur.UtilisateurWrite;
import topmodel.jpa.sample.demo.entities.securite.profil.Droit;
import topmodel.jpa.sample.demo.entities.securite.profil.Profil;
import topmodel.jpa.sample.demo.entities.securite.utilisateur.SecuriteUtilisateurMappers;
import topmodel.jpa.sample.demo.entities.securite.utilisateur.TypeUtilisateur;
import topmodel.jpa.sample.demo.entities.securite.utilisateur.Utilisateur;
import topmodel.jpa.sample.demo.enums.securite.utilisateur.TypeUtilisateurCode;

public class SecuriteUtilisateurMappersTest {

    @Test
    public void testCreateUtilisateurRead() {
        // GIVEN
        Utilisateur utilisateur = new Utilisateur();
        utilisateur.setId(1);
        utilisateur.setNom("Doe");
        utilisateur.setPrenom("John");
        utilisateur.setEmail("john.doe@example.com");
        utilisateur.setDateNaissance(LocalDate.of(1990, 1, 1));
        utilisateur.setAdresse("123 Main St");
        utilisateur.setActif(true);
        var profil = new Profil();
        profil.setId(2);
        profil.setDroits(Arrays.asList(Droit.CREATE, Droit.DELETE));
        utilisateur.setProfil(profil);
        utilisateur.setTypeUtilisateur(TypeUtilisateur.from(TypeUtilisateurCode.ADMIN));

        // WHEN
        UtilisateurRead utilisateurRead = SecuriteUtilisateurMappers.createUtilisateurRead(utilisateur, null);

        // THEN
        assertThat(utilisateurRead.getId()).isEqualTo(utilisateur.getId());
        assertThat(utilisateurRead.getNom()).isEqualTo(utilisateur.getNom());
        assertThat(utilisateurRead.getPrenom()).isEqualTo(utilisateur.getPrenom());
        assertThat(utilisateurRead.getEmail()).isEqualTo(utilisateur.getEmail());
        assertThat(utilisateurRead.getDateNaissance()).isEqualTo(utilisateur.getDateNaissance());
        assertThat(utilisateurRead.getAdresse()).isEqualTo(utilisateur.getAdresse());
        assertThat(utilisateurRead.getActif()).isEqualTo(utilisateur.getActif());
        assertThat(utilisateurRead.getProfilId()).isEqualTo(utilisateur.getProfil().getId());
        assertThat(utilisateurRead.getTypeUtilisateurCode()).isEqualTo(utilisateur.getTypeUtilisateur().getCode());
    }

    @Test
    public void testToUtilisateur() {
        // GIVEN
        UtilisateurWrite source = new UtilisateurWrite();
        source.setNom("Doe");
        source.setPrenom("John");
        source.setEmail("john.doe@example.com");
        source.setDateNaissance(LocalDate.of(1990, 1, 1));
        source.setAdresse("123 Main St");
        source.setActif(true);
        source.setTypeUtilisateurCode(TypeUtilisateurCode.ADMIN);

        // WHEN
        Utilisateur target = SecuriteUtilisateurMappers.toUtilisateur(source, null);

        // THEN
        assertThat(target.getNom()).isEqualTo(source.getNom());
        assertThat(target.getPrenom()).isEqualTo(source.getPrenom());
        assertThat(target.getEmail()).isEqualTo(source.getEmail());
        assertThat(target.getDateNaissance()).isEqualTo(source.getDateNaissance());
        assertThat(target.getAdresse()).isEqualTo(source.getAdresse());
        assertThat(target.getActif()).isEqualTo(source.getActif());
        assertThat(target.getTypeUtilisateur().getCode()).isEqualTo(source.getTypeUtilisateurCode());
    }

    @Test
    public void testToUtilisateurWithNullSource() {
        // WHEN & THEN
        assertThatThrownBy(() -> SecuriteUtilisateurMappers.toUtilisateur(null, null)).isInstanceOf(IllegalArgumentException.class);
    }

    @Test
    public void testCreateUtilisateurReadWithNullUtilisateur() {
        // WHEN & THEN
        assertThatThrownBy(() -> SecuriteUtilisateurMappers.createUtilisateurRead(null, null)).isInstanceOf(IllegalArgumentException.class);
    }
}
