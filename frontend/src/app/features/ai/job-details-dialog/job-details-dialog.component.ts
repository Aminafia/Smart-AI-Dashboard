import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AIJob } from '../../../core/models/ai/ai-job.model';
import { MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { DatePipe } from '@angular/common';
import { AiService } from '../../../core/services/ai.service';
import { JobStatusResponse } from '../../../core/models/ai/job-status-response.model';
import { Observable, map } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { MarkdownService } from '../../../shared/services/markdown.service';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  standalone: true,
  selector: 'app-job-details-dialog',
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatChipsModule,
    DatePipe,
    AsyncPipe
  ],
  templateUrl: './job-details-dialog.component.html',
  styleUrl: './job-details-dialog.component.css'
})
export class JobDetailsDialogComponent {

  jobStatus$!: Observable<JobStatusResponse>;
  renderedResult = '';

  constructor(
    @Inject(MAT_DIALOG_DATA)
    public jobId: string,
    private aiService: AiService,
    private markdownService: MarkdownService

  ) { }

  ngOnInit(): void {
    this.jobStatus$ = this.aiService
                              .getStatus(this.jobId)
                              .pipe(
                                map(response => {
                                  const job = response.data;
                                  this.renderedResult = this.markdownService.render(job.result ?? '');
                                  return job;
                                })
                              );
  }

}