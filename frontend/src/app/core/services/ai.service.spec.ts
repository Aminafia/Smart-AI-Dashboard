import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { SKIP_LOADING } from '../constants/loading-context';
import { AiService } from './ai.service';

describe('AiService', () => {
  let service: AiService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/AI`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('posts a generation request', () => {
    const request = { prompt: 'Explain RAG' };

    service.generate(request as never).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/generate`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('gets job status without activating the global loading indicator', () => {
    service.getStatus('job-123').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/status/job-123`);
    expect(req.request.method).toBe('GET');
    expect(req.request.context.get(SKIP_LOADING)).toBe(true);
    req.flush({});
  });

  it('posts a text summarization request', () => {
    const request = { text: 'Long text to summarize' };

    service.summarize(request as never).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/summarize`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('summarizes a document with an empty body and skips global loading', () => {
    service.summarizeDocument('document-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/documents/document-1/summarize`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();
    expect(req.request.context.get(SKIP_LOADING)).toBe(true);
    req.flush({});
  });

  it('requests paginated jobs using the supplied page values', () => {
    service.getJobs(2, 25).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/jobs?page=2&pageSize=25`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('requests AI statistics', () => {
    service.getStats().subscribe();

    const req = httpMock.expectOne(`${apiUrl}/stats`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('loads a saved document summary without global loading', () => {
    service.getDocumentSummary('document-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/documents/document-1/summary`);
    expect(req.request.method).toBe('GET');
    expect(req.request.context.get(SKIP_LOADING)).toBe(true);
    req.flush({});
  });

  it('surfaces HTTP failures to the subscriber', () => {
    let receivedStatus = 0;

    service.getStats().subscribe({
      error: error => receivedStatus = error.status
    });

    httpMock.expectOne(`${apiUrl}/stats`).flush(
      { message: 'Unavailable' },
      { status: 503, statusText: 'Service Unavailable' }
    );

    expect(receivedStatus).toBe(503);
  });
});
