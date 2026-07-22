import { Component, OnInit } from '@angular/core';
import { AiService } from '../../../core/services/ai.service';
import { AIJob } from '../../../core/models/ai/ai-job.model';
import { MatTableModule } from '@angular/material/table';
import { DatePipe } from '@angular/common';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { PageCardComponent } from '../../../shared/components/page-card/page-card.component';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { JobDetailsDialogComponent } from '../job-details-dialog/job-details-dialog.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-job-history',
  standalone: true,
  imports: [
    MatTableModule,
    MatChipsModule,
    DatePipe,
    PageHeaderComponent,
    PageCardComponent,
    MatIconModule,
    MatButtonModule,
    MatDialogModule
  ],
  templateUrl: './job-history.component.html',
  styleUrl: './job-history.component.css',
})

export class JobHistoryComponent implements OnInit {

  jobs: AIJob[] = [];
  displayedColumns: string[] = [
    'jobType',
    'prompt',
    'status',
    'createdAt',
    'actions'
  ];

  constructor(
    private aiService: AiService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.aiService.getJobs(1, 10).subscribe({
      next: (response) => {
        this.jobs = response.data;
        console.log(this.jobs);
      }
    });
  }

viewJob(job: AIJob): void {
  this.dialog.open(JobDetailsDialogComponent, {
    width: '800px',
    data: job.id
  });
}
}
