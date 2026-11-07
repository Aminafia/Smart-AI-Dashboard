import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';

import { TokenService } from '../services/token.service';
import { guestGuard } from './guest.guard';

describe('guestGuard', () => {
  let tokenService: TokenService;
  let router: { navigate: ReturnType<typeof vi.fn> };

  const executeGuard = () => TestBed.runInInjectionContext(() =>
    guestGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot)
  );

  beforeEach(() => {
    localStorage.clear();
    router = { navigate: vi.fn().mockResolvedValue(true) };

    TestBed.configureTestingModule({
      providers: [
        TokenService,
        { provide: Router, useValue: router }
      ]
    });
    tokenService = TestBed.inject(TokenService);
  });

  afterEach(() => localStorage.clear());

  it('allows navigation for a visitor without a token', () => {
    expect(executeGuard()).toBe(true);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('redirects authenticated users to the dashboard', () => {
    tokenService.saveToken('jwt-token');

    expect(executeGuard()).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });
});
