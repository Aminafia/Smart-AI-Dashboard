import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { CurrentUserService } from './current-user.service';
import { TokenService } from './token.service';
import { AuthStateService } from './auth-state.service';

describe('AuthStateService', () => {
  let service: AuthStateService;
  let tokenService: { isLoggedIn: ReturnType<typeof vi.fn>; removeToken: ReturnType<typeof vi.fn> };
  let currentUserService: { clearCurrentUser: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    tokenService = { isLoggedIn: vi.fn(), removeToken: vi.fn() };
    currentUserService = { clearCurrentUser: vi.fn() };
    router = { navigate: vi.fn().mockResolvedValue(true) };

    TestBed.configureTestingModule({
      providers: [
        AuthStateService,
        { provide: TokenService, useValue: tokenService },
        { provide: CurrentUserService, useValue: currentUserService },
        { provide: Router, useValue: router }
      ]
    });
    service = TestBed.inject(AuthStateService);
  });

  it('delegates the logged-in check to TokenService', () => {
    tokenService.isLoggedIn.mockReturnValue(true);

    expect(service.isLoggedIn()).toBe(true);
    expect(tokenService.isLoggedIn).toHaveBeenCalledOnce();
  });

  it('returns false when there is no active token', () => {
    tokenService.isLoggedIn.mockReturnValue(false);

    expect(service.isLoggedIn()).toBe(false);
  });

  it('clears authentication state and navigates to login on logout', () => {
    service.logout();

    expect(tokenService.removeToken).toHaveBeenCalledOnce();
    expect(currentUserService.clearCurrentUser).toHaveBeenCalledOnce();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
