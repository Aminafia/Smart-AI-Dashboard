import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { AiService } from '../../../core/services/ai.service';
import { MarkdownService } from '../../../shared/services/markdown.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';
import { SummarizeComponent } from './summarize.component';

describe('SummarizeComponent', () => {
  let component: SummarizeComponent;
  let fixture: ComponentFixture<SummarizeComponent>;
  let aiService: { summarize: ReturnType<typeof vi.fn> };
  let markdown: { render: ReturnType<typeof vi.fn> };
  let snackbar: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    aiService = { summarize: vi.fn() };
    markdown = { render: vi.fn(value => `<p>${value}</p>`) };
    snackbar = { success: vi.fn(), error: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [SummarizeComponent],
      providers: [
        { provide: AiService, useValue: aiService },
        { provide: MarkdownService, useValue: markdown },
        { provide: SnackbarService, useValue: snackbar }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SummarizeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('submits text and renders the returned summary', () => {
    aiService.summarize.mockReturnValue(of({
      success: true,
      message: 'Summary generated',
      data: { output: 'A concise summary', isFallback: false }
    }));
    component.summarizeForm.controls.text.setValue('Long source document');

    component.summarize();

    expect(aiService.summarize).toHaveBeenCalledWith({ text: 'Long source document' });
    expect(component.resultText).toBe('A concise summary');
    expect(component.renderedResult).toContain('A concise summary');
    expect(markdown.render).toHaveBeenCalledWith('A concise summary');
    expect(snackbar.success).toHaveBeenCalledWith('Summary generated');
  });

  it('clears previous output and error state before summarizing again', () => {
    aiService.summarize.mockReturnValue(of({
      success: true,
      message: 'Done',
      data: { output: 'New summary', isFallback: false }
    }));
    component.errorMessage = 'Previous error';
    component.resultText = 'Old result';
    component.renderedResult = '<p>Old result</p>';

    component.summarize();

    expect(component.errorMessage).toBe('');
    expect(component.resultText).toBe('New summary');
    expect(component.renderedResult).toContain('New summary');
  });

  it('shows a user-facing error when summarization fails', () => {
    aiService.summarize.mockReturnValue(throwError(() => new Error('Provider unavailable')));

    component.summarize();

    expect(component.errorMessage).toBe('Failed to generate summary');
    expect(snackbar.error).toHaveBeenCalledWith('Failed to generate summary');
  });
});
