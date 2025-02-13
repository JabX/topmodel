////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////

import {EntityToType, FieldEntry2} from "ngx-focus-entities";
import {DO_CODE_LISTE, DO_LIBELLE} from "@/domains";

import {DroitCode} from "./references";

export type ProfilWrite = EntityToType<ProfilWriteEntityType>;
export interface ProfilWriteEntityType {
    libelle: FieldEntry2<typeof DO_LIBELLE, string>;
    droits: FieldEntry2<typeof DO_CODE_LISTE, DroitCode[]>;
}

export const ProfilWriteEntity: ProfilWriteEntityType = {
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
    }
};
