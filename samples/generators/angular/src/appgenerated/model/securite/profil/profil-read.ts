////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

import {EntityToType, FieldEntry2, ListEntry} from "ngx-focus-entities";
import {DO_CODE_LISTE, DO_DATE_HEURE, DO_ID, DO_LIBELLE} from "@/domains";

import {UtilisateurItemEntity, UtilisateurItemEntityType} from "../utilisateur/utilisateur-item";
import {DroitCode} from "./references";

export type ProfilRead = EntityToType<ProfilReadEntityType>;
export interface ProfilReadEntityType {
    id: FieldEntry2<typeof DO_ID, number>;
    libelle: FieldEntry2<typeof DO_LIBELLE, string>;
    droits: FieldEntry2<typeof DO_CODE_LISTE, DroitCode[]>;
    dateCreation: FieldEntry2<typeof DO_DATE_HEURE, string>;
    dateModification: FieldEntry2<typeof DO_DATE_HEURE, string>;
    utilisateurs: ListEntry<UtilisateurItemEntityType>;
}

export const ProfilReadEntity: ProfilReadEntityType = {
    id: {
        type: "field",
        name: "id",
        domain: DO_ID,
        isRequired: false,
        label: "securite.profil.profil.id"
    },
    libelle: {
        type: "field",
        name: "libelle",
        domain: DO_LIBELLE,
        isRequired: true,
        label: "securite.profil.profil.libelle"
    },
    droits: {
        type: "field",
        name: "droits",
        domain: DO_CODE_LISTE,
        isRequired: false,
        label: "securite.profil.profil.droits"
    },
    dateCreation: {
        type: "field",
        name: "dateCreation",
        domain: DO_DATE_HEURE,
        isRequired: true,
        label: "common.entityListeners.dateCreation"
    },
    dateModification: {
        type: "field",
        name: "dateModification",
        domain: DO_DATE_HEURE,
        isRequired: false,
        label: "common.entityListeners.dateModification"
    },
    utilisateurs: {
        type: "list",
        entity: UtilisateurItemEntity
    }
};
