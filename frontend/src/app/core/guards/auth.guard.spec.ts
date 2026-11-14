import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';

import { TokenService } from '../services/token.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let tokenService: TokenService;
  let router: { navigate: ReturnType<typeof vi.fn> };

  const executeGuard = () =>
    TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );

  beforeEach(() => {
    localStorage.clear();
    router = { navigate: vi.fn().mockResolvedValue(true) };

    TestBed.configureTestingModule({
      providers: [TokenService, { provide: Router, useValue: router }],
    });
    tokenService = TestBed.inject(TokenService);
  });

  afterEach(() => localStorage.clear());

  it('allows navigation when a token exists', () => {
    tokenService.saveToken('jwt-token');

    expect(executeGuard()).toBe(true);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('blocks navigation and redirects to login when no token exists', () => {
    expect(executeGuard()).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
