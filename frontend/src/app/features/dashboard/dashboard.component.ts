import { Component, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { PageCardComponent } from '../../shared/components/page-card/page-card.component';
import { AiService } from '../../core/services/ai.service';
import { AIStats } from '../../core/models/ai/ai-stats.model';

@Component({
  selector: 'app-dashboard',
  imports: [
    PageHeaderComponent,
    PageCardComponent,
    MatIconModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {

  stats: AIStats = {
    totalJobs: 0,
    completedJobs: 0,
    processingJobs: 0,
    failedJobs: 0
  };

  constructor(
    private readonly aiService: AiService
  ) {}

  ngOnInit(): void {
    this.loadStats();
  }

  private loadStats(): void {
    this.aiService.getStats().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.stats = response.data;
        }
      },
      error: () => {
        // Keep default zero values if statistics cannot be loaded.
      }
    });
  }
}