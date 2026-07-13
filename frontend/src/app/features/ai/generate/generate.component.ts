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
import { GenerateResponse } from '../../../core/models/generate-response';
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

  isLoading = false;

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
    this.isLoading = false;
    this.errorMessage = '';

    const request: GenerateRequest = {
      prompt: this.generateForm.controls.prompt.value
    };

    this.aiService.generate(request)
      .subscribe({
        next: (response) => {
          this.jobId = response.jobId;
          this.currentStatus = response.status;
          this.isLoading = true;
          this.snackbarService.success('AI job submitted successfully');
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
        takeWhile(response => 
          response.status !== 'Completed' && response.status !== 'Failed',
          true)
      )
      .subscribe({
        next: (response: JobStatusResponse) => {

          this.currentStatus = response.status;
          this.cdr.detectChanges();

          if (response.status === 'Completed') {
            this.resultText = response.result ?? '';
            this.renderedResult = this.markdownService.render(this.resultText);
            this.isLoading = false;
            this.cdr.detectChanges();
          }

          if (response.status === 'Failed') {
            this.resultText = response.error ?? 'Unknown error';
            this.renderedResult = this.markdownService.render(this.resultText);
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        }
      });

  }
  
}