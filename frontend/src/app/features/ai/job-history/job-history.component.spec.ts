import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';

import { AiService } from '../../../core/services/ai.service';
import { JobHistoryComponent } from './job-history.component';

describe('JobHistoryComponent', () => {
  let component: JobHistoryComponent;
  let fixture: ComponentFixture<JobHistoryComponent>;
  let aiService: { getJobs: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    aiService = { getJobs: vi.fn().mockReturnValue(of({ data: { items: [], totalCount: 0 } })) };

    await TestBed.configureTestingModule({
      imports: [JobHistoryComponent],
      providers: [
        { provide: AiService, useValue: aiService },
        { provide: MatDialog, useValue: { open: vi.fn() } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(JobHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads the first page of jobs when initialized', () => {
    expect(aiService.getJobs).toHaveBeenCalledWith(1, 10);
    expect(component.jobs).toEqual([]);
    expect(component.totalCount).toBe(0);
  });

  it('updates pagination and reloads jobs when the page changes', () => {
    component.onPageChange({ pageIndex: 1, pageSize: 25, length: 100 });

    expect(component.page).toBe(2);
    expect(component.pageSize).toBe(25);
    expect(aiService.getJobs).toHaveBeenLastCalledWith(2, 25);
  });

  it('returns zero range values when no jobs exist', () => {
    expect(component.startItem).toBe(0);
    expect(component.endItem).toBe(0);
  });
});
