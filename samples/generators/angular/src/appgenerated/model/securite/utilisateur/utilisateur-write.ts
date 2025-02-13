////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

import {EntityToType, FieldEntry2} from "ngx-focus-entities";
import {DO_BOOLEEN, DO_CODE, DO_DATE, DO_EMAIL, DO_ID, DO_LIBELLE} from "@/domains";

import {TypeUtilisateurCode} from "./references";

export type UtilisateurWrite = EntityToType<UtilisateurWriteEntityType>;
export interface UtilisateurWriteEntityType {
    nom: FieldEntry2<typeof DO_LIBELLE, string>;
    prenom: FieldEntry2<typeof DO_LIBELLE, string>;
    email: FieldEntry2<typeof DO_EMAIL, string>;
    dateNaissance: FieldEntry2<typeof DO_DATE, string>;
    adresse: FieldEntry2<typeof DO_LIBELLE, string>;
    actif: FieldEntry2<typeof DO_BOOLEEN, boolean>;
    profilId: FieldEntry2<typeof DO_ID, number>;
    typeUtilisateurCode: FieldEntry2<typeof DO_CODE, TypeUtilisateurCode>;
}

export const UtilisateurWriteEntity: UtilisateurWriteEntityType = {
    nom: {
        type: "field",
        name: "nom",
        domain: DO_LIBELLE,
        isRequired: true,
        label: "securite.utilisateur.utilisateur.nom"
    },
    prenom: {
        type: "field",
        name: "prenom",
        domain: DO_LIBELLE,
        isRequired: true,
        label: "securite.utilisateur.utilisateur.prenom"
    },
    email: {
        type: "field",
        name: "email",
        domain: DO_EMAIL,
        isRequired: true,
        label: "securite.utilisateur.utilisateur.email"
    },
    dateNaissance: {
        type: "field",
        name: "dateNaissance",
        domain: DO_DATE,
        isRequired: false,
        label: "securite.utilisateur.utilisateur.dateNaissance"
    },
    adresse: {
        type: "field",
        name: "adresse",
        domain: DO_LIBELLE,
        isRequired: false,
        label: "securite.utilisateur.utilisateur.adresse"
    },
    actif: {
        type: "field",
        name: "actif",
        domain: DO_BOOLEEN,
        defaultValue: true,
        isRequired: true,
        label: "securite.utilisateur.utilisateur.actif"
    },
    profilId: {
        type: "field",
        name: "profilId",
        domain: DO_ID,
        isRequired: true,
        label: "securite.utilisateur.utilisateur.profilId"
    },
    typeUtilisateurCode: {
        type: "field",
        name: "typeUtilisateurCode",
        domain: DO_CODE,
        defaultValue: "GEST",
        isRequired: true,
        label: "securite.utilisateur.utilisateur.typeUtilisateurCode"
    }
};
