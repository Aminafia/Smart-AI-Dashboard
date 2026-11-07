import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { SKIP_LOADING } from '../constants/loading-context';
import { DocumentService } from './document.service';

describe('DocumentService', () => {
  let service: DocumentService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/Documents`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DocumentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('uploads the selected file as multipart form data', () => {
    const file = new File(['document'], 'report.pdf', { type: 'application/pdf' });

    service.upload(file).subscribe();

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeInstanceOf(FormData);
    expect((req.request.body as FormData).get('file')).toBe(file);
    req.flush({});
  });

  it('requests paginated documents', () => {
    service.getDocuments(3, 15).subscribe();

    const req = httpMock.expectOne(`${apiUrl}?page=3&pageSize=15`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('requests a document by ID', () => {
    service.getDocument('doc-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/doc-1`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('downloads a document as a Blob and skips global loading', () => {
    service.download('doc-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/doc-1`);
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    expect(req.request.context.get(SKIP_LOADING)).toBe(true);
    req.flush(new Blob(['pdf-content'], { type: 'application/pdf' }));
  });

  it('deletes a document by ID', () => {
    service.delete('doc-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/doc-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('requests text extraction without global loading', () => {
    service.extract('doc-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/doc-1/extract`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();
    expect(req.request.context.get(SKIP_LOADING)).toBe(true);
    req.flush(null);
  });

  it('loads extracted content without global loading', () => {
    service.getContent('doc-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/doc-1/content`);
    expect(req.request.method).toBe('GET');
    expect(req.request.context.get(SKIP_LOADING)).toBe(true);
    req.flush({});
  });

  it('propagates upload errors to the subscriber', () => {
    let receivedStatus = 0;

    service.getDocuments(1, 10).subscribe({
      error: (error) => (receivedStatus = error.status),
    });

    httpMock
      .expectOne(`${apiUrl}?page=1&pageSize=10`)
      .flush({ message: 'Forbidden' }, { status: 403, statusText: 'Forbidden' });

    expect(receivedStatus).toBe(403);
  });
});
