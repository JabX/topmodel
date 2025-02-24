import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UtilisateurItem } from '../../../model/securite/utilisateur/utilisateur-item';
import { UtilisateurRead } from '../../../model/securite/utilisateur/utilisateur-read';
import { UtilisateurWrite } from '../../../model/securite/utilisateur/utilisateur-write';
import { UtilisateurService } from './utilisateur.service';

describe('UtilisateurService', () => {
  let service: UtilisateurService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        UtilisateurService,
      ],
    });
    service = TestBed.inject(UtilisateurService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add an utilisateur', () => {
    const dummyUtilisateur: UtilisateurWrite = { /* mock data */ };
    const dummyResponse: UtilisateurRead = { /* mock data */ };

    service.addUtilisateur(dummyUtilisateur).subscribe(response => {
      expect(response).toEqual(dummyResponse);
    });

    const req = httpMock.expectOne('/api/utilisateurs');
    expect(req.request.method).toBe('POST');
    req.flush(dummyResponse);
  });

  it('should delete an utilisateur', () => {
    const utiId = 1;

    service.deleteUtilisateur(utiId).subscribe(response => {
      expect(response).toBeNull();
    });

    const req = httpMock.expectOne(`/api/utilisateurs/${utiId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should get an utilisateur by id', () => {
    const dummyUtilisateur: UtilisateurRead = { /* mock data */ };
    const utiId = 1;

    service.getUtilisateur(utiId).subscribe(response => {
      expect(response).toEqual(dummyUtilisateur);
    });

    const req = httpMock.expectOne(`/api/utilisateurs/${utiId}`);
    expect(req.request.method).toBe('GET');
    req.flush(dummyUtilisateur);
  });

  it('should search utilisateurs', () => {
    const dummyUtilisateurs: UtilisateurItem[] = [ /* mock data */ ];

    service.searchUtilisateur('Doe', 'John', undefined, undefined, undefined, undefined, undefined, undefined).subscribe(response => {
      expect(response).toEqual(dummyUtilisateurs);
    });

    const req = httpMock.expectOne(request => request.url.includes('/api/utilisateurs') && request.params.has('nom') && request.params.has('prenom'));
    expect(req.request.method).toBe('GET');
    req.flush(dummyUtilisateurs);
  });

  it('should update an utilisateur', () => {
    const dummyUtilisateur: UtilisateurWrite = { /* mock data */ };
    const dummyResponse: UtilisateurRead = { /* mock data */ };
    const utiId = 1;

    service.updateUtilisateur(utiId, dummyUtilisateur).subscribe(response => {
      expect(response).toEqual(dummyResponse);
    });

    const req = httpMock.expectOne(`/api/utilisateurs/${utiId}`);
    expect(req.request.method).toBe('PUT');
    req.flush(dummyResponse);
  });
});
