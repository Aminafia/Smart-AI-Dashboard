import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { AiService } from '../../core/services/ai.service';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let aiService: { getStats: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    aiService = { getStats: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [{ provide: AiService, useValue: aiService }]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('loads and displays AI statistics on initialization', () => {
    aiService.getStats.mockReturnValue(of({
      success: true,
      data: { totalJobs: 12, completedJobs: 8, processingJobs: 2, failedJobs: 2 }
    }));

    fixture.detectChanges();

    expect(aiService.getStats).toHaveBeenCalledOnce();
    expect(component.stats).toEqual({
      totalJobs: 12,
      completedJobs: 8,
      processingJobs: 2,
      failedJobs: 2
    });
  });

  it('keeps default zero values when the API reports failure', () => {
    aiService.getStats.mockReturnValue(of({
      success: false,
      data: { totalJobs: 99, completedJobs: 99, processingJobs: 0, failedJobs: 0 }
    }));

    fixture.detectChanges();

    expect(component.stats.totalJobs).toBe(0);
    expect(component.stats.completedJobs).toBe(0);
  });

  it('keeps default zero values when the statistics request errors', () => {
    aiService.getStats.mockReturnValue(throwError(() => new Error('Unavailable')));

    fixture.detectChanges();

    expect(component.stats).toEqual({
      totalJobs: 0,
      completedJobs: 0,
      processingJobs: 0,
      failedJobs: 0
    });
  });
});
