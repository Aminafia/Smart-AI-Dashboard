import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { LoginRequest } from '../models/login-request';
import { LoginResponse } from '../models/login-response';
import { ApiResponse } from '../models/api-response';
import { TokenService } from './token.service';
import { tap } from 'rxjs/operators';
@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'http://localhost:5260/api/auth';

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
  ) {}

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
return this.http
  .post<ApiResponse<LoginResponse>>(
    `${this.apiUrl}/login`,
    request
  )
  .pipe(
    tap(response => {
      this.tokenService.saveToken(response.data.token);
    })
  );
  }
}