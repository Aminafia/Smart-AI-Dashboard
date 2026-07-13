import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { HttpContext } from '@angular/common/http';
import { SKIP_LOADING } from '../constants/loading-context';

import { GenerateRequest } from '../models/generate-request';
import { GenerateResponse } from '../models/generate-response';
import { JobStatusResponse } from '../models/job-status-response';

@Injectable({
  providedIn: 'root'
})

export class AiService {

  private apiUrl = 'http://localhost:5260/api/AI';

  constructor(
    private http: HttpClient,
  ) { }

  generate(request: GenerateRequest): Observable<GenerateResponse> {
    return this.http.post<GenerateResponse>(
      `${this.apiUrl}/generate`,
      request
    );
  }

  getStatus(jobId: string): Observable<JobStatusResponse> {
    return this.http.get<JobStatusResponse>(
      `${this.apiUrl}/status/${jobId}`,
      { context: new HttpContext().set(SKIP_LOADING, true) }
    );
  }
}