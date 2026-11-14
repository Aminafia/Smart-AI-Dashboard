import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { SKIP_LOADING } from '../constants/loading-context';
import { LoadingService } from '../services/loading.service';
import { loadingInterceptor } from './loading.interceptor';

describe('loadingInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let loadingService: LoadingService;
  let loadingState = false;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([loadingInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    loadingService = TestBed.inject(LoadingService);
    loadingService.loading$.subscribe((value) => (loadingState = value));
  });

  afterEach(() => httpMock.verify());

  it('shows loading while a request is pending and hides it after success', () => {
    http.get('/api/data').subscribe();

    expect(loadingState).toBe(true);

    httpMock.expectOne('/api/data').flush({});

    expect(loadingState).toBe(false);
  });

  it('hides loading when a request fails', () => {
    http.get('/api/data').subscribe({ error: () => undefined });

    expect(loadingState).toBe(true);

    httpMock
      .expectOne('/api/data')
      .flush({ message: 'Failure' }, { status: 500, statusText: 'Server Error' });

    expect(loadingState).toBe(false);
  });

  it('does not show loading for requests with SKIP_LOADING context', () => {
    http.get('/api/silent', { context: new HttpContext().set(SKIP_LOADING, true) }).subscribe();

    expect(loadingState).toBe(false);

    httpMock.expectOne('/api/silent').flush({});

    expect(loadingState).toBe(false);
  });

  it('keeps loading visible until all concurrent requests complete', () => {
    http.get('/api/first').subscribe();
    http.get('/api/second').subscribe();

    const requests = httpMock.match((request) => request.url.startsWith('/api/'));
    expect(requests).toHaveLength(2);
    expect(loadingState).toBe(true);

    requests[0].flush({});
    expect(loadingState).toBe(true);

    requests[1].flush({});
    expect(loadingState).toBe(false);
  });
});
