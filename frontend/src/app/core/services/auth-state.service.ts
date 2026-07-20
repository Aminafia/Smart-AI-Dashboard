import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { TokenService } from './token.service';
import { CurrentUserService } from './current-user.service';

@Injectable({
  providedIn: 'root'
})
export class AuthStateService {

  constructor(
    private tokenService: TokenService,
    private currentUserService: CurrentUserService,
    private router: Router
  ) { }

  isLoggedIn(): boolean {
    return this.tokenService.isLoggedIn();
  }

  logout(): void {
    this.tokenService.removeToken();
    this.currentUserService.clearCurrentUser();
    this.router.navigate(['/login']);
  }
}