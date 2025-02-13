import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ProfilItem } from '../../../model/securite/profil/profil-item';
import { ProfilRead } from '../../../model/securite/profil/profil-read';
import { ProfilWrite } from '../../../model/securite/profil/profil-write';
import { ProfilService } from './profil.service';

describe('ProfilService', () => {
  let service: ProfilService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ProfilService,
      ],
    });
    service = TestBed.inject(ProfilService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add a profil', () => {
    const dummyProfil: ProfilWrite = {
      /* mock data */
    };
    const dummyResponse: ProfilRead = {
      /* mock data */
    };

    service.addProfil(dummyProfil).subscribe((response) => {
      expect(response).toEqual(dummyResponse);
    });

    const req = httpMock.expectOne('/api/profils');
    expect(req.request.method).toBe('POST');
    req.flush(dummyResponse);
  });

  it('should get a profil by id', () => {
    const dummyProfil: ProfilRead = {
      /* mock data */
    };
    const proId = 1;

    service.getProfil(proId).subscribe((response) => {
      expect(response).toEqual(dummyProfil);
    });

    const req = httpMock.expectOne(`/api/profils/${proId}`);
    expect(req.request.method).toBe('GET');
    req.flush(dummyProfil);
  });

  it('should get all profils', () => {
    const dummyProfils: ProfilItem[] = [
      /* mock data */
    ];

    service.getProfils().subscribe((response) => {
      expect(response).toEqual(dummyProfils);
    });

    const req = httpMock.expectOne('/api/profils');
    expect(req.request.method).toBe('GET');
    req.flush(dummyProfils);
  });

  it('should update a profil', () => {
    const dummyProfil: ProfilWrite = {
      /* mock data */
    };
    const dummyResponse: ProfilRead = {
      /* mock data */
    };
    const proId = 1;

    service.updateProfil(proId, dummyProfil).subscribe((response) => {
      expect(response).toEqual(dummyResponse);
    });

    const req = httpMock.expectOne(`/api/profils/${proId}`);
    expect(req.request.method).toBe('PUT');
    req.flush(dummyResponse);
  });
});
