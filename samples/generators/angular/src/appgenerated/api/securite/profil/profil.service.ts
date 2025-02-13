////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////


import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ProfilItem } from "../../../model/securite/profil/profil-item";
import { ProfilRead } from "../../../model/securite/profil/profil-read";
import { ProfilWrite } from "../../../model/securite/profil/profil-write";
@Injectable({
    providedIn: 'root'
})
export class ProfilService {

    private readonly http = inject(HttpClient);

    /**
     * @description Ajoute un Profil
     * @param profil Profil à sauvegarder
     * @returns Profil sauvegardé
     */
    addProfil(profil: ProfilWrite): Observable<ProfilRead> {
        return this.http.post<ProfilRead>(`/api/profils`, profil);
    }

    /**
     * @description Charge le détail d'un Profil
     * @param proId Id technique
     * @returns Le détail de l'Profil
     */
    getProfil(proId: number): Observable<ProfilRead> {
        return this.http.get<ProfilRead>(`/api/profils/${proId}`);
    }

    /**
     * @description Liste tous les Profils
     * @returns Profils matchant les critères
     */
    getProfils(): Observable<ProfilItem[]> {
        return this.http.get<ProfilItem[]>(`/api/profils`);
    }

    /**
     * @description Sauvegarde un Profil
     * @param proId Id technique
     * @param profil Profil à sauvegarder
     * @returns Profil sauvegardé
     */
    updateProfil(proId: number, profil: ProfilWrite): Observable<ProfilRead> {
        return this.http.put<ProfilRead>(`/api/profils/${proId}`, profil);
    }
}
