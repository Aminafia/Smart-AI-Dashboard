import { Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SKIP_LOADING } from '../constants/loading-context';
import { GenerateRequest } from '../models/ai/generate-request.model';
import { GenerateResponse } from '../models/ai/generate-response.model';
import { SummarizeRequest } from '../models/ai/summarize-request.model';
import { SummarizeResponse } from '../models/ai/summarize-response.model';
import { JobStatusResponse } from '../models/ai/job-status-response.model';
import { ApiResponse } from '../models/shared/api-response';
import { AIJob } from '../models/ai/ai-job.model';
import { PagedResponse } from '../models/shared/paged-response.model';

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private readonly apiUrl = `${environment.apiUrl}/AI`;

  constructor(private readonly http: HttpClient) {}

  generate(request: GenerateRequest): Observable<ApiResponse<GenerateResponse>> {
    return this.http.post<ApiResponse<GenerateResponse>>(
      `${this.apiUrl}/generate`, request);
  }

  getStatus(jobId: string): Observable<ApiResponse<JobStatusResponse>> {
    return this.http.get<ApiResponse<JobStatusResponse>>(
      `${this.apiUrl}/status/${jobId}`,
      { context: new HttpContext().set(SKIP_LOADING, true) });
  }

  summarize(request: SummarizeRequest): Observable<ApiResponse<SummarizeResponse>> {
    return this.http.post<ApiResponse<SummarizeResponse>>(
      `${this.apiUrl}/summarize`, request);
  }

  summarizeDocument(documentId: string): Observable<ApiResponse<SummarizeResponse>> {
    return this.http.post<ApiResponse<SummarizeResponse>>(
      `${this.apiUrl}/documents/${documentId}/summarize`, null);
  }

  getJobs(page: number, pageSize: number): Observable<ApiResponse<PagedResponse<AIJob>>> {
    return this.http.get<ApiResponse<PagedResponse<AIJob>>>(
      `${this.apiUrl}/jobs?page=${page}&pageSize=${pageSize}`);
  }
}
