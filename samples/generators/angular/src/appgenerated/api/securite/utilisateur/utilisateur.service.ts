////
//// ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
////


import { HttpClient, HttpParams } from "@angular/common/http";
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
    addUtilisateur(utilisateur: UtilisateurWrite): Observable<UtilisateurRead> {
        return this.http.post<UtilisateurRead>(`/api/utilisateurs`, utilisateur);
    }

    /**
     * @description Supprime un utilisateur
     * @param utiId Id de l'utilisateur
     */
    deleteUtilisateur(utiId: number): Observable<void> {
        return this.http.delete<void>(`/api/utilisateurs/${utiId}`);
    }

    /**
     * @description Charge le détail d'un utilisateur
     * @param utiId Id de l'utilisateur
     * @returns Le détail de l'utilisateur
     */
    getUtilisateur(utiId: number): Observable<UtilisateurRead> {
        return this.http.get<UtilisateurRead>(`/api/utilisateurs/${utiId}`);
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
    searchUtilisateur(nom?: string, prenom?: string, email?: string, dateNaissance?: string, adresse?: string, actif?: boolean, profilId?: number, typeUtilisateurCode?: TypeUtilisateurCode, queryParams: any = {}): Observable<UtilisateurItem[]> {
        if (nom !== null && nom !== undefined) {
            queryParams['nom'] = nom
        }

        if (prenom !== null && prenom !== undefined) {
            queryParams['prenom'] = prenom
        }

        if (email !== null && email !== undefined) {
            queryParams['email'] = email
        }

        if (dateNaissance !== null && dateNaissance !== undefined) {
            queryParams['dateNaissance'] = dateNaissance
        }

        if (adresse !== null && adresse !== undefined) {
            queryParams['adresse'] = adresse
        }

        if (actif !== null && actif !== undefined) {
            queryParams['actif'] = actif
        }

        if (profilId !== null && profilId !== undefined) {
            queryParams['profilId'] = profilId
        }

        if (typeUtilisateurCode !== null && typeUtilisateurCode !== undefined) {
            queryParams['typeUtilisateurCode'] = typeUtilisateurCode
        }

        const httpParams = new HttpParams({fromObject: queryParams});
        const httpOptions = {params: httpParams}

        return this.http.get<UtilisateurItem[]>(`/api/utilisateurs`, httpOptions);
    }

    /**
     * @description Sauvegarde un utilisateur
     * @param utiId Id de l'utilisateur
     * @param utilisateur Utilisateur à sauvegarder
     * @returns Utilisateur sauvegardé
     */
    updateUtilisateur(utiId: number, utilisateur: UtilisateurWrite): Observable<UtilisateurRead> {
        return this.http.put<UtilisateurRead>(`/api/utilisateurs/${utiId}`, utilisateur);
    }
}
