import { TestBed } from '@angular/core/testing';

import { TokenService } from './token.service';

describe('TokenService', () => {
  let service: TokenService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenService);
  });

  afterEach(() => localStorage.clear());

  it('stores a token under the application token key', () => {
    service.saveToken('jwt-token');

    expect(localStorage.getItem('token')).toBe('jwt-token');
  });

  it('returns the stored token', () => {
    localStorage.setItem('token', 'jwt-token');

    expect(service.getToken()).toBe('jwt-token');
  });

  it('returns null when no token exists', () => {
    expect(service.getToken()).toBeNull();
  });

  it('removes the stored token', () => {
    service.saveToken('jwt-token');

    service.removeToken();

    expect(service.getToken()).toBeNull();
  });

  it('reports logged-in state only when a token exists', () => {
    expect(service.isLoggedIn()).toBe(false);

    service.saveToken('jwt-token');

    expect(service.isLoggedIn()).toBe(true);

    service.removeToken();

    expect(service.isLoggedIn()).toBe(false);
  });
});
