import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { WorkspaceSectionComponent } from '../../../shared/components/workspace-section/workspace-section.component';
import { ResultViewerComponent } from '../../../shared/components/result-viewer/result-viewer.component';

import { AiService } from '../../../core/services/ai.service';
import { SummarizeRequest } from '../../../core/models/summarize-request';
import { MarkdownService } from '../../../shared/services/markdown.service';
import { SnackbarService } from '../../../shared/services/snackbar.service';


@Component({
  selector: 'app-summarize',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    PageHeaderComponent,
    WorkspaceSectionComponent,
    ResultViewerComponent
  ],
  templateUrl: './summarize.component.html',
  styleUrl: './summarize.component.css'
})

export class SummarizeComponent {

  errorMessage = '';
  resultText = '';
  renderedResult = '';
  summarizeForm = new FormGroup({
    text: new FormControl('', { nonNullable: true })
  });

  constructor(
    private aiService: AiService,
    private markdownService: MarkdownService,
    private snackbarService: SnackbarService
  ) { }

  summarize(): void {

  this.errorMessage = '';
  this.resultText = '';
  this.renderedResult = '';

  const request: SummarizeRequest = {
    text: this.summarizeForm.controls.text.value
  };

  this.aiService.summarize(request)
    .subscribe({
      next: (response) => {
        const data = response.data!;

        this.resultText = data.output;
        this.renderedResult = this.markdownService.render(data.output);
        this.snackbarService.success(response.message);
      },
      error: () => {
        this.errorMessage = 'Failed to generate summary';
        this.snackbarService.error(this.errorMessage);
      }
    });

  }


}
