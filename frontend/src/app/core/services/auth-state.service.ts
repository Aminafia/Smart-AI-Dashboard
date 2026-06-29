import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class AuthStateService {

  constructor(
    private tokenService: TokenService,
    private router: Router
  ) {}

  isLoggedIn(): boolean {
    return this.tokenService.isLoggedIn();
  }

  logout(): void {
    this.tokenService.removeToken();
    this.router.navigate(['/login']);
  }
}