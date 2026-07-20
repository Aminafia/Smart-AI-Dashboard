import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { LoginRequest } from '../models/login-request';
import { LoginResponse } from '../models/login-response';
import { ApiResponse } from '../models/api-response';
import { TokenService } from './token.service';
import { CurrentUserService } from './current-user.service';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private currentUserService: CurrentUserService,
  ) {}

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> 
  {
    return this.http
      .post<ApiResponse<LoginResponse>>(
        `${this.apiUrl}/login`,
        request)
      .pipe(tap(response => 
        { this.tokenService.saveToken(response.data.token);
          this.currentUserService.refreshCurrentUser();
         }));
  }
}