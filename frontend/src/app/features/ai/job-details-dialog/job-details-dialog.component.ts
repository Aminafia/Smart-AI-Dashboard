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

@Component({
  standalone: true,
  selector: 'app-job-details-dialog',
  imports: [
    MatDialogModule,
    MatButtonModule,
    DatePipe,
    AsyncPipe
  ],
  templateUrl: './job-details-dialog.component.html',
  styleUrl: './job-details-dialog.component.css'
})
export class JobDetailsDialogComponent {

jobStatus$!: Observable<JobStatusResponse>;

  constructor(
    @Inject(MAT_DIALOG_DATA)
    public jobId: string,
    private aiService: AiService,

  ) { }

ngOnInit(): void {
  this.jobStatus$ = this.aiService
    .getStatus(this.jobId)
    .pipe(
      map(response => response.data)
    );
}

}