////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

import {EntityToType, FieldEntry2} from "ngx-focus-entities";
import {DO_CODE, DO_EMAIL, DO_ID, DO_LIBELLE} from "@/domains";

import {TypeUtilisateurCode} from "./references";

export type UtilisateurItem = EntityToType<UtilisateurItemEntityType>;
export interface UtilisateurItemEntityType {
    id: FieldEntry2<typeof DO_ID, number>;
    nom: FieldEntry2<typeof DO_LIBELLE, string>;
    prenom: FieldEntry2<typeof DO_LIBELLE, string>;
    email: FieldEntry2<typeof DO_EMAIL, string>;
    typeUtilisateurCode: FieldEntry2<typeof DO_CODE, TypeUtilisateurCode>;
}

export const UtilisateurItemEntity: UtilisateurItemEntityType = {
    id: {
        type: "field",
        name: "id",
        domain: DO_ID,
        isRequired: false,
        label: "securite.utilisateur.utilisateur.id"
    },
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
    typeUtilisateurCode: {
        type: "field",
        name: "typeUtilisateurCode",
        domain: DO_CODE,
        defaultValue: "GEST",
        isRequired: true,
        label: "securite.utilisateur.utilisateur.typeUtilisateurCode"
    }
};
