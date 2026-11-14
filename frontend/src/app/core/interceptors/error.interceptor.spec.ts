import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { SnackbarService } from '../../shared/services/snackbar.service';
import { AuthStateService } from '../services/auth-state.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let snackbar: { error: ReturnType<typeof vi.fn>; warning: ReturnType<typeof vi.fn> };
  let authState: { logout: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    snackbar = { error: vi.fn(), warning: vi.fn() };
    authState = { logout: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: SnackbarService, useValue: snackbar },
        { provide: AuthStateService, useValue: authState },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('shows validation errors returned with a 400 response', () => {
    http.get('/api/invalid').subscribe({ error: () => undefined });

    httpMock
      .expectOne('/api/invalid')
      .flush(
        { errors: ['Email is invalid', 'Password is required'] },
        { status: 400, statusText: 'Bad Request' },
      );

    expect(snackbar.warning).toHaveBeenCalledWith(['Email is invalid', 'Password is required']);
  });

  it('shows the API message for a 404 response', () => {
    http.get('/api/missing').subscribe({ error: () => undefined });

    httpMock
      .expectOne('/api/missing')
      .flush({ message: 'Document not found' }, { status: 404, statusText: 'Not Found' });

    expect(snackbar.warning).toHaveBeenCalledWith('Document not found');
  });

  it('logs the user out when the API returns 401', () => {
    http.get('/api/private').subscribe({ error: () => undefined });

    httpMock
      .expectOne('/api/private')
      .flush({ message: 'Session expired' }, { status: 401, statusText: 'Unauthorized' });

    expect(snackbar.error).toHaveBeenCalledWith('Session expired');
    expect(authState.logout).toHaveBeenCalledOnce();
  });

  it('shows a fallback message when the server returns no error body', () => {
    http.get('/api/unavailable').subscribe({ error: () => undefined });

    httpMock.expectOne('/api/unavailable').flush(null, { status: 500, statusText: 'Server Error' });

    expect(snackbar.error).toHaveBeenCalledWith('Something went wrong.');
  });

  it('shows a connection message for network failures', () => {
    http.get('/api/offline').subscribe({ error: () => undefined });

    httpMock.expectOne('/api/offline').error(new ProgressEvent('error'));

    expect(snackbar.error).toHaveBeenCalledWith('Unable to connect to the server.');
  });
});
