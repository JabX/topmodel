////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////


import { HttpClient, HttpContext, HttpHeaders, HttpParams } from "@angular/common/http";
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
    addProfil(profil: ProfilWrite, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<ProfilRead> {
        return this.http.post<ProfilRead>(`/api/profils`, profil, {observe: 'body', ...options});
    }

    /**
     * @description Charge le détail d'un Profil
     * @param proId Id technique
     * @returns Le détail de l'Profil
     */
    getProfil(proId: number, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<ProfilRead> {
        return this.http.get<ProfilRead>(`/api/profils/${proId}`, {observe: 'body', ...options});
    }

    /**
     * @description Liste tous les Profils
     * @returns Profils matchant les critères
     */
    getProfils(options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<ProfilItem[]> {
        return this.http.get<ProfilItem[]>(`/api/profils`, {observe: 'body', ...options});
    }

    /**
     * @description Sauvegarde un Profil
     * @param proId Id technique
     * @param profil Profil à sauvegarder
     * @returns Profil sauvegardé
     */
    updateProfil(proId: number, profil: ProfilWrite, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<ProfilRead> {
        return this.http.put<ProfilRead>(`/api/profils/${proId}`, profil, {observe: 'body', ...options});
    }
}
