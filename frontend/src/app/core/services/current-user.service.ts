import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';

import { TokenService } from './token.service';
import { CurrentUser } from '../models/current-user';
import { JwtClaims } from '../constants/jwt-claims';

@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {

  constructor(
    private tokenService: TokenService
  ) {}

  getCurrentUser(): CurrentUser | null {

    const token = this.tokenService.getToken();

    if (!token) {
      return null;
    }

    const decoded: any = jwtDecode(token);

    return {
      userId: decoded[JwtClaims.UserId],
      email: decoded[JwtClaims.Email],
      fullName: decoded[JwtClaims.FullName],
      role: decoded[JwtClaims.Role]
    };
  }

  getEmail(): string | null {
    return this.getCurrentUser()?.email ?? null;
  }

  getFullName(): string | null {
    return this.getCurrentUser()?.fullName ?? null;
  }

  getRole(): string | null {
    return this.getCurrentUser()?.role ?? null;
  }

  getUserId(): string | null {
    return this.getCurrentUser()?.userId ?? null;
  }
}