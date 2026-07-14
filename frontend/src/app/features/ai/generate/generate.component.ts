/*
=========================================================
Execution Flow
=========================================================
User clicks Generate button on UI
  |
  ↓
GenerateComponent.onGenerate() is called
  |
  |takes GenerateRequest(prompt) from form and calls AIService
  ↓
AIService.generate()
  |
  ↓
HttpClient → sends GenerateRequest(prompt) to backend API
  |
  | GenerateRequest(prompt)
  ↓
(backend processing request, Status="Processing")
  |          ↓
  |   ↓ AuthInterceptor - adds JWT token to the request header
  |   ↓ LoadingInterceptor - shows loading loading overlay through spinner while waiting for the response and hides when response is received
  |     ErrorInterceptor - handles errors and displays snackbar messages through SnackbarService 
  |          ↓
  |   GenerateComponent.pollJobStatus() - polls the AI service for the status of the job until it is completed or failed.
  |
(backend processed request, Status="Completed" or "Failed")
  |
  |GenerateResponse(jobId, status) and returns to GenerateComponent while still passing through all interceptors  
  | 
  ↓ ErrorInterceptors - handles http errors if any
  ↓ LoadingInterceptor - hides loading overlay through spinner
  ↓
GenerateComponent.pollJobStatus() - polls the AI service for the status of the job until it is completed or failed.
  ↓
MarkdownService.render() - renders the result or error message to the user in markdown format 
  |
  |DOMPurify - sanitizes the rendered HTML to prevent XSS attacks
  ↓
Displayed to UI through generated.component.html
*/

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

import { interval } from 'rxjs';
import { switchMap, takeWhile } from 'rxjs/operators';
import { ChangeDetectorRef } from '@angular/core';


import { GenerateRequest } from '../../../core/models/generate-request';
import { JobStatusResponse } from '../../../core/models/job-status-response';

import { AiService } from '../../../core/services/ai.service';
import { MarkdownService } from '../../../shared/services/markdown.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';

@Component({
  selector: 'app-generate',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './generate.component.html',
  styleUrl: './generate.component.css'
})

export class GenerateComponent {

  errorMessage = '';

  resultText = '';
  renderedResult = '';
  currentStatus = '';
  jobId = '';

    generateForm = new FormGroup({
    prompt: new FormControl('', { nonNullable: true })
  });

  constructor(
    private aiService: AiService,
    private cdr: ChangeDetectorRef,
    private markdownService: MarkdownService,
    private snackbarService: SnackbarService
  ) { }

  onGenerate(): void {

    this.resultText = '';
    this.renderedResult = '';
    this.currentStatus = '';
    this.jobId = '';
    this.errorMessage = '';

    const request: GenerateRequest = {
      prompt: this.generateForm.controls.prompt.value
    };

    this.aiService.generate(request)
      .subscribe({
        next: (response) => {
          const responseData = response.data!;
          this.jobId = responseData.jobId;
          this.currentStatus = responseData.status;
          this.snackbarService.success(response.message);
          this.pollJobStatus();
        },
        error: (error) => {
          console.error(error);
          this.snackbarService.error('Failed to submit AI job');
          this.errorMessage = 'Failed to submit AI job';
        }
      });

  }
  

  pollJobStatus(): void {

    interval(2000)
      .pipe(
        switchMap(() => this.aiService.getStatus(this.jobId)),
        takeWhile(responseData => 
          responseData.data.status !== 'Completed' && 
          responseData.data.status !== 'Failed', 
          true))
      .subscribe({
        next: (responseData) => {

          const job = responseData.data;

          this.currentStatus = responseData.data.status;
          this.cdr.detectChanges();

          if (responseData.data.status === 'Completed') {
            this.resultText = responseData.data.result ?? '';
            this.renderedResult = this.markdownService.render(this.resultText);
            this.cdr.detectChanges();
          }

          if (responseData.data.status === 'Failed') {
            this.resultText = responseData.data.error ?? 'Unknown error';
            this.renderedResult = this.markdownService.render(this.resultText);
            this.cdr.detectChanges();
          }
        }
      });

  }
  
}