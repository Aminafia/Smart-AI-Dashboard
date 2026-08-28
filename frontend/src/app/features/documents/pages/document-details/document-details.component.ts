import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';

import { DocumentModel } from '../../../../core/models/documents/document.model';
import { DocumentContentResponse } from '../../../../core/models/documents/document-content-response.model';
import { DocumentService } from '../../../../core/services/document.service';
import { AiService } from '../../../../core/services/ai.service';
import { SnackbarService } from '../../../../shared/services/snackbar.service';

import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { PageCardComponent } from '../../../../shared/components/page-card/page-card.component';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-document-details',
  standalone: true,
  imports: [
    DatePipe,
    PageHeaderComponent,
    PageCardComponent,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './document-details.component.html',
  styleUrl: './document-details.component.css'
})
export class DocumentDetailsComponent implements OnInit {
  document: DocumentModel | null = null;
  content: DocumentContentResponse | null = null;

  loadingContent = false;
  extracting = false;
  summarizing = false;
  summary = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly documentService: DocumentService,
    private readonly aiService: AiService,
    private readonly snackbarService: SnackbarService
  ) {}

  ngOnInit(): void {
    this.loadDocument();
  }

  private loadDocument(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigate(['/documents']);
      return;
    }

    this.documentService.getDocument(id).subscribe({
      next: document => {
        this.document = document;
        this.loadContent(id);
      },
      error: () => {
        this.snackbarService.error('Unable to load document.');
        this.router.navigate(['/documents']);
      }
    });
  }

  loadContent(id: string): void {
    this.loadingContent = true;

    this.documentService.getContent(id).subscribe({
      next: response => {
        this.content = response;
        this.loadingContent = false;
      },
      error: () => {
        this.content = null;
        this.loadingContent = false;
      }
    });
  }

  extractDocument(): void {
    if (!this.document) return;

    this.extracting = true;
    this.documentService.extract(this.document.id).subscribe({
      next: () => {
        this.extracting = false;
        this.snackbarService.success('Document text extracted successfully.');
        this.loadContent(this.document!.id);
      },
      error: () => {
        this.extracting = false;
        this.snackbarService.error('Unable to extract document text.');
      }
    });
  }

  summarizeDocument(): void {
    if (!this.document || !this.content?.extractedText) return;

    this.summarizing = true;
    this.summary = '';

    this.aiService.summarizeDocument(this.document.id).subscribe({
      next: response => {
        this.summarizing = false;
        this.summary = response.data?.output ?? '';
        this.snackbarService.success('Document summary generated successfully.');
      },
      error: () => {
        this.summarizing = false;
        this.snackbarService.error('Unable to summarize document.');
      }
    });
  }

  downloadDocument(): void {
    if (!this.document) return;

    this.documentService.download(this.document.id).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = this.document!.fileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => this.snackbarService.error('Unable to download document.')
    });
  }

  goBack(): void {
    this.router.navigate(['/documents']);
  }
}
