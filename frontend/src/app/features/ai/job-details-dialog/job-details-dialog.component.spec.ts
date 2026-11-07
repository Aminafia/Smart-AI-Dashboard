import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

import { AiService } from '../../../core/services/ai.service';
import { MarkdownService } from '../../../shared/services/markdown.service';
import { JobDetailsDialogComponent } from './job-details-dialog.component';

describe('JobDetailsDialogComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobDetailsDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: 'job-123' },
        { provide: AiService, useValue: { getStatus: vi.fn() } },
        { provide: MarkdownService, useValue: { render: vi.fn() } },
      ],
    }).compileComponents();
  });

  it('receives the selected job ID from the dialog data token', () => {
    const fixture = TestBed.createComponent(JobDetailsDialogComponent);

    expect(fixture.componentInstance.jobId).toBe('job-123');
  });
});
