import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NEVER, of, throwError } from 'rxjs';

import { AIJobStatus } from '../../../core/models/ai/ai-job-status.model';
import { AiService } from '../../../core/services/ai.service';
import { MarkdownService } from '../../../shared/services/markdown.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { GenerateComponent } from './generate.component';

describe('GenerateComponent', () => {
  let component: GenerateComponent;
  let fixture: ComponentFixture<GenerateComponent>;
  let aiService: { generate: ReturnType<typeof vi.fn>; getStatus: ReturnType<typeof vi.fn> };
  let markdown: { render: ReturnType<typeof vi.fn> };
  let snackbar: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    aiService = { generate: vi.fn(), getStatus: vi.fn() };
    markdown = { render: vi.fn((value) => `<p>${value}</p>`) };
    snackbar = { success: vi.fn(), error: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [GenerateComponent],
      providers: [
        { provide: AiService, useValue: aiService },
        { provide: MarkdownService, useValue: markdown },
        { provide: SnackbarService, useValue: snackbar },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GenerateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('submits the prompt and starts status polling when the API accepts the job', () => {
    aiService.generate.mockReturnValue(
      of({
        success: true,
        message: 'Job queued',
        data: { jobId: 'job-1', status: AIJobStatus.Pending },
      }),
    );
    const poll = vi.spyOn(component, 'pollJobStatus').mockImplementation(() => undefined);
    component.generateForm.controls.prompt.setValue('Explain RAG');

    component.onGenerate();

    expect(aiService.generate).toHaveBeenCalledWith({ prompt: 'Explain RAG' });
    expect(component.jobId).toBe('job-1');
    expect(component.currentStatus).toBe(AIJobStatus.Pending);
    expect(snackbar.success).toHaveBeenCalledWith('Job queued');
    expect(poll).toHaveBeenCalledOnce();
  });

  it('clears old output and errors before starting a new request', () => {
    aiService.generate.mockReturnValue(
      of({
        success: true,
        message: 'Queued',
        data: { jobId: 'job-2', status: AIJobStatus.Pending },
      }),
    );
    vi.spyOn(component, 'pollJobStatus').mockImplementation(() => undefined);
    component.resultText = 'Old result';
    component.renderedResult = '<p>Old result</p>';
    component.errorMessage = 'Old error';

    component.onGenerate();

    expect(component.resultText).toBe('');
    expect(component.renderedResult).toBe('');
    expect(component.errorMessage).toBe('');
  });

  it('shows an error when the generation request fails', () => {
    vi.spyOn(console, 'error').mockImplementation(() => undefined);
    aiService.generate.mockReturnValue(throwError(() => new Error('Network failure')));

    component.onGenerate();

    expect(component.errorMessage).toBe('Failed to submit AI job');
    expect(snackbar.error).toHaveBeenCalledWith('Failed to submit AI job');
  });

  it('renders the completed job result after polling', () => {
    vi.useFakeTimers();
    aiService.getStatus.mockReturnValue(
      of({
        data: { status: AIJobStatus.Completed, result: '**Generated result**' },
      }),
    );
    component.jobId = 'job-1';

    component.pollJobStatus();
    vi.advanceTimersByTime(2000);

    expect(component.currentStatus).toBe(AIJobStatus.Completed);
    expect(component.resultText).toBe('**Generated result**');
    expect(markdown.render).toHaveBeenCalledWith('**Generated result**');
    expect(component.renderedResult).toContain('Generated result');
    vi.useRealTimers();
  });

  it('renders the failure message when the polled job fails', () => {
    vi.useFakeTimers();
    aiService.getStatus.mockReturnValue(
      of({
        data: { status: AIJobStatus.Failed, error: 'Provider failed' },
      }),
    );
    component.jobId = 'job-2';

    component.pollJobStatus();
    vi.advanceTimersByTime(2000);

    expect(component.currentStatus).toBe(AIJobStatus.Failed);
    expect(component.resultText).toBe('Provider failed');
    expect(markdown.render).toHaveBeenCalledWith('Provider failed');
    vi.useRealTimers();
  });

  it('stops polling when the component is destroyed', () => {
    vi.useFakeTimers();
    aiService.getStatus.mockReturnValue(NEVER);
    component.jobId = 'job-3';

    component.pollJobStatus();
    vi.advanceTimersByTime(2000);
    expect(aiService.getStatus).toHaveBeenCalledOnce();

    component.ngOnDestroy();
    vi.advanceTimersByTime(4000);

    expect(aiService.getStatus).toHaveBeenCalledOnce();
    vi.useRealTimers();
  });

  it('maps every known job status to readable text', () => {
    expect(component.getStatusText(AIJobStatus.Pending)).toBe('Pending');
    expect(component.getStatusText(AIJobStatus.Processing)).toBe('Processing');
    expect(component.getStatusText(AIJobStatus.Completed)).toBe('Completed');
    expect(component.getStatusText(AIJobStatus.Failed)).toBe('Failed');
    expect(component.getStatusText(AIJobStatus.Retrying)).toBe('Retrying');
    expect(component.getStatusText(999 as unknown as AIJobStatus)).toBe('Unknown');
  });
});
