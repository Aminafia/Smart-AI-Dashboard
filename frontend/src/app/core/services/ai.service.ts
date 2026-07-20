import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { HttpContext } from '@angular/common/http';
import { SKIP_LOADING } from '../constants/loading-context';

import { GenerateRequest } from '../models/generate-request';
import { GenerateResponse } from '../models/generate-response';
import { JobStatusResponse } from '../models/job-status-response';
import { ApiResponse } from '../models/api-response';
import { SummarizeRequest } from '../models/summarize-request';
import { SummarizeResponse } from '../models/summarize-response';

@Injectable({
  providedIn: 'root'
})

export class AiService {

  private apiUrl = `${environment.apiUrl}/AI`;

  constructor(
    private http: HttpClient,
  ) { }

  generate(request: GenerateRequest): Observable<ApiResponse<GenerateResponse>> {
    return this.http.post<ApiResponse<GenerateResponse>>(
      `${this.apiUrl}/generate`,
      request
    );
  }

  getStatus(jobId: string): Observable<ApiResponse<JobStatusResponse>> {
    return this.http.get<ApiResponse<JobStatusResponse>>(
      `${this.apiUrl}/status/${jobId}`,
      { context: new HttpContext().set(SKIP_LOADING, true) }
    );
  }


  summarize(request: SummarizeRequest): Observable<ApiResponse<SummarizeResponse>> {
    return this.http.post<ApiResponse<SummarizeResponse>>(
      `${this.apiUrl}/summarize`,
      request
    );
  }


}