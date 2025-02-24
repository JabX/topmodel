////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////


import { HttpClient, HttpContext, HttpHeaders, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { TypeUtilisateurCode } from "../../../model/securite/utilisateur/references";
import { UtilisateurItem } from "../../../model/securite/utilisateur/utilisateur-item";
import { UtilisateurRead } from "../../../model/securite/utilisateur/utilisateur-read";
import { UtilisateurWrite } from "../../../model/securite/utilisateur/utilisateur-write";
@Injectable({
    providedIn: 'root'
})
export class UtilisateurService {

    private readonly http = inject(HttpClient);

    /**
     * @description Ajoute un utilisateur
     * @param utilisateur Utilisateur à sauvegarder
     * @returns Utilisateur sauvegardé
     */
    addUtilisateur(utilisateur: UtilisateurWrite, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; reportProgress?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<UtilisateurRead> {
        return this.http.post<UtilisateurRead>(`/api/utilisateurs`, utilisateur, {observe: 'body', ...options});
    }

    /**
     * @description Supprime un utilisateur
     * @param utiId Id de l'utilisateur
     */
    deleteUtilisateur(utiId: number, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; reportProgress?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<void> {
        return this.http.delete<void>(`/api/utilisateurs/${utiId}`, {observe: 'body', ...options});
    }

    /**
     * @description Charge le détail d'un utilisateur
     * @param utiId Id de l'utilisateur
     * @returns Le détail de l'utilisateur
     */
    getUtilisateur(utiId: number, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; reportProgress?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<UtilisateurRead> {
        return this.http.get<UtilisateurRead>(`/api/utilisateurs/${utiId}`, {observe: 'body', ...options});
    }

    /**
     * @description Recherche des utilisateurs
     * @param nom Nom de l'utilisateur
     * @param prenom Nom de l'utilisateur
     * @param email Email de l'utilisateur
     * @param dateNaissance Age de l'utilisateur
     * @param adresse Adresse de l'utilisateur
     * @param actif Si l'utilisateur est actif
     * @param profilId Profil de l'utilisateur
     * @param typeUtilisateurCode Type d'utilisateur
     * @returns Utilisateurs matchant les critères
     */
    searchUtilisateur(nom?: string, prenom?: string, email?: string, dateNaissance?: string, adresse?: string, actif?: boolean, profilId?: number, typeUtilisateurCode?: TypeUtilisateurCode, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; reportProgress?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<UtilisateurItem[]> {
        const addParam = (key: string, value: any) => {
          if (value !== null && value !== undefined) {
            if (options.params instanceof HttpParams) {
              options.params = options.params.append(key, value);
            } else {
              if (!options.params) {
                options.params = {};
              }
              options.params[key] = value;
            }
          }
        };
        addParam('nom', nom);
        addParam('prenom', prenom);
        addParam('email', email);
        addParam('dateNaissance', dateNaissance);
        addParam('adresse', adresse);
        addParam('actif', actif);
        addParam('profilId', profilId);
        addParam('typeUtilisateurCode', typeUtilisateurCode);

        return this.http.get<UtilisateurItem[]>(`/api/utilisateurs`, {observe: 'body', ...options});
    }

    /**
     * @description Sauvegarde un utilisateur
     * @param utiId Id de l'utilisateur
     * @param utilisateur Utilisateur à sauvegarder
     * @returns Utilisateur sauvegardé
     */
    updateUtilisateur(utiId: number, utilisateur: UtilisateurWrite, options: {headers?: HttpHeaders | {[header: string]: string | string[]}; context?: HttpContext; params?: HttpParams | {[param: string]: string | number | boolean | ReadonlyArray<string | number | boolean>}; withCredentials?: boolean; reportProgress?: boolean; transferCache?: {includeHeaders?: string[]} | boolean} = {}): Observable<UtilisateurRead> {
        return this.http.put<UtilisateurRead>(`/api/utilisateurs/${utiId}`, utilisateur, {observe: 'body', ...options});
    }
}
