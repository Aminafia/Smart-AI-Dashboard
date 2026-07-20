import { Injectable, signal, computed } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

import { TokenService } from './token.service';
import { CurrentUser } from '../models/current-user';
import { JwtClaims } from '../constants/jwt-claims';
import { JwtPayload } from '../models/jwt-payload';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {

  constructor(
    private tokenService: TokenService
  ) {
    this.loadCurrentUser();
  }

  private readonly _currentUser = signal<CurrentUser | null>(null);
  readonly currentUser = this._currentUser.asReadonly();
  readonly fullName = computed(() => this._currentUser()?.fullName ?? '');
  readonly email = computed(() => this._currentUser()?.email ?? '');
  readonly role = computed(() => this._currentUser()?.role ?? '');


  private loadCurrentUser(): void {

    const token = this.tokenService.getToken();

    if (!token) {
      this._currentUser.set(null);
      return;
    }

    const decoded = jwtDecode<JwtPayload>(token);

    this._currentUser.set({
      userId: decoded[JwtClaims.UserId],
      email: decoded[JwtClaims.Email],
      fullName: decoded[JwtClaims.FullName],
      role: decoded[JwtClaims.Role]
    });
  }

  refreshCurrentUser(): void {
    this.loadCurrentUser();
  }

  clearCurrentUser(): void {
    this._currentUser.set(null);
  }

}