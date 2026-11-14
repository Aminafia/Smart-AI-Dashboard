import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AIJobStatus } from '../../../core/models/ai/ai-job-status.model';
import { AIJob } from '../../../core/models/ai/ai-job.model';
import { AiService } from '../../../core/services/ai.service';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PageCardComponent } from '../../../shared/components/page-card/page-card.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { JobDetailsDialogComponent } from '../job-details-dialog/job-details-dialog.component';

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
    MatDialogModule,
    EmptyStateComponent,
    MatTooltipModule,
    MatPaginatorModule,
  ],
  templateUrl: './job-history.component.html',
  styleUrl: './job-history.component.css',
})
export class JobHistoryComponent implements OnInit {
  jobs: AIJob[] = [];
  displayedColumns: string[] = ['jobType', 'prompt', 'status', 'createdAt', 'actions'];
  page = 1;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [5, 10, 25, 50];

  private pollingSubscription?: Subscription;

  constructor(
    private aiService: AiService,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.aiService.getJobs(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.jobs = response.data.items;
        this.totalCount = response.data.totalCount;

        if (this.hasRunningJobs()) {
          this.startPolling();
        } else {
          this.stopPolling();
        }
      },
    });
  }

  private hasRunningJobs(): boolean {
    return this.jobs.some(
      (job) =>
        job.status === AIJobStatus.Pending ||
        job.status === AIJobStatus.Processing ||
        job.status === AIJobStatus.Retrying,
    );
  }

  private startPolling(): void {
    if (this.pollingSubscription) {
      return;
    }
    this.pollingSubscription = interval(5000)
      .pipe(switchMap(() => this.aiService.getJobs(this.page, this.pageSize)))
      .subscribe({
        next: (response) => {
          this.jobs = response.data.items;
          this.totalCount = response.data.totalCount;
          if (!this.hasRunningJobs()) {
            this.stopPolling();
          }
        },
      });
  }

  private stopPolling(): void {
    this.pollingSubscription?.unsubscribe();
    this.pollingSubscription = undefined;
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  viewJob(job: AIJob): void {
    this.dialog.open(JobDetailsDialogComponent, {
      width: '900px',
      maxHeight: '85vh',
      data: job.id,
    });
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadJobs();
  }

  get startItem(): number {
    return this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }
}
