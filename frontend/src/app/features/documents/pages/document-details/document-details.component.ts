import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import { DocumentModel } from '../../../../core/models/documents/document.model';
import { DocumentContentResponse } from '../../../../core/models/documents/document-content-response.model';
import { DocumentService } from '../../../../core/services/document.service';
import { AiService } from '../../../../core/services/ai.service';
import { SnackbarService } from '../../../../shared/services/snackbar.service';
import { PageCardComponent } from '../../../../shared/components/page-card/page-card.component';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-document-details',
  standalone: true,
  imports: [DatePipe, PageCardComponent, MatButtonModule, MatIconModule],
  templateUrl: './document-details.component.html',
  styleUrl: './document-details.component.css'
})
export class DocumentDetailsComponent implements OnInit, OnDestroy {
  @ViewChild('workspaceAnchor') private workspaceAnchor?: ElementRef<HTMLElement>;

  document: DocumentModel | null = null;
  content: DocumentContentResponse | null = null;

  loadingDocument = true;
  loadingPreview = false;
  pdfPreviewUrl: SafeResourceUrl | null = null;
  previewVisible = true;

  workspaceVisible = false;
  extracting = false;
  loadingContent = false;
  summarizing = false;
  summary = '';
  contentError = '';
  summaryError = '';

  private previewObjectUrl: string | null = null;
  private currentDocumentId: string | null = null;
  private extractionInFlight = false;
  private contentRequestInFlight = false;
  private summaryInFlight = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly documentService: DocumentService,
    private readonly aiService: AiService,
    private readonly snackbarService: SnackbarService,
    private readonly sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      void this.router.navigate(['/documents']);
      return;
    }

    this.currentDocumentId = id;

    // Start the file request immediately. Do not wait for metadata or text requests.
    this.loadPdfPreview(id);

    this.documentService.getDocuments(1, 100).subscribe({
      next: response => {
        const found = response.items.find(item => item.id === id);
        if (!found) {
          this.loadingDocument = false;
          this.snackbarService.error('Unable to load document details.');
          void this.router.navigate(['/documents']);
          return;
        }
        this.document = found;
        this.loadingDocument = false;
      },
      error: () => {
        this.loadingDocument = false;
        this.snackbarService.error('Unable to load document details.');
      }
    });
  }

  ngOnDestroy(): void {
    if (this.previewObjectUrl) {
      URL.revokeObjectURL(this.previewObjectUrl);
      this.previewObjectUrl = null;
    }
  }

  private loadPdfPreview(id: string): void {
    this.loadingPreview = true;
    this.documentService.download(id).subscribe({
      next: blob => {
        if (this.currentDocumentId !== id) return;
        if (this.previewObjectUrl) URL.revokeObjectURL(this.previewObjectUrl);
        this.previewObjectUrl = URL.createObjectURL(blob);
        this.pdfPreviewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.previewObjectUrl);
        this.loadingPreview = false;
      },
      error: () => {
        this.loadingPreview = false;
        this.snackbarService.error('Unable to load document preview.');
      }
    });
  }

  togglePreview(): void {
    this.previewVisible = !this.previewVisible;
  }

  downloadDocument(): void {
    const file = this.document;
    if (!file) return;

    this.documentService.download(file.id).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = file.fileName;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
      },
      error: () => this.snackbarService.error('Unable to download document.')
    });
  }

  extractDocument(): void {
    const id = this.currentDocumentId;
    if (!id || this.extractionInFlight || this.extracting) return;

    this.revealWorkspace();
    this.extractionInFlight = true;
    this.extracting = true;
    this.contentError = '';
    this.summaryError = '';

    this.documentService.extract(id).subscribe({
      next: () => {
        this.extractionInFlight = false;
        this.extracting = false;
        // Fetch the persisted extraction only after the API confirms it has finished.
        this.fetchContent(id, true);
      },
      error: () => {
        this.extractionInFlight = false;
        this.extracting = false;
        this.contentError = 'Text extraction failed. Please try again.';
        this.snackbarService.error('Unable to extract document text.');
      }
    });
  }

  summarizeDocument(): void {
    const id = this.currentDocumentId;
    if (!id || this.summaryInFlight || this.summarizing) return;

    this.revealWorkspace();
    this.summaryError = '';
    this.contentError = '';

    // Reuse a saved extraction. If none exists yet, extract before summarizing.
    if (this.content?.extractedText?.trim()) {
      this.generateSummary(id);
      return;
    }

    this.loadingContent = true;
    this.fetchContent(id, false, () => {
      if (this.content?.extractedText?.trim()) {
        this.generateSummary(id);
      } else {
        this.extractThenSummarize(id);
      }
    });
  }

  private extractThenSummarize(id: string): void {
    if (this.extractionInFlight || this.extracting) return;

    this.extractionInFlight = true;
    this.extracting = true;
    this.documentService.extract(id).subscribe({
      next: () => {
        this.extractionInFlight = false;
        this.extracting = false;
        this.fetchContent(id, true, () => {
          if (this.content?.extractedText?.trim()) this.generateSummary(id);
          else this.summaryError = 'No text was found in this document to summarize.';
        });
      },
      error: () => {
        this.extractionInFlight = false;
        this.extracting = false;
        this.loadingContent = false;
        this.contentError = 'Text extraction failed. Please try Extract Text again.';
        this.summaryError = 'The summary could not be generated because extraction failed.';
      }
    });
  }

  private fetchContent(id: string, scrollWhenReady: boolean, afterLoad?: () => void): void {
    if (this.contentRequestInFlight) {
      // Avoid racing content requests. Continue the dependent action after the active request ends.
      if (afterLoad) {
        const waitForExistingRequest = () => {
          if (this.contentRequestInFlight) {
            requestAnimationFrame(waitForExistingRequest);
            return;
          }
          afterLoad();
        };
        requestAnimationFrame(waitForExistingRequest);
      }
      return;
    }

    this.contentRequestInFlight = true;
    this.loadingContent = true;
    this.documentService.getContent(id).subscribe({
      next: response => {
        this.content = response;
        this.contentError = '';
        this.loadingContent = false;
        this.contentRequestInFlight = false;
        if (scrollWhenReady) this.scrollToWorkspace();
        afterLoad?.();
      },
      error: () => {
        this.content = null;
        this.loadingContent = false;
        this.contentRequestInFlight = false;
        if (scrollWhenReady) this.scrollToWorkspace();
        // A missing extraction is expected when Summarize is clicked first.
        if (afterLoad) afterLoad();
        else this.contentError = 'Extracted text is not available yet. Please extract the document again.';
      }
    });
  }

  private generateSummary(id: string): void {
    if (this.summaryInFlight || this.summarizing) return;
    this.summaryInFlight = true;
    this.summarizing = true;
    this.summary = '';
    this.summaryError = '';

    this.aiService.summarizeDocument(id).subscribe({
      next: response => {
        this.summary = response.data?.output?.trim() ?? '';
        this.summarizing = false;
        this.summaryInFlight = false;
        if (!this.summary) {
          this.summaryError = 'The summary request completed, but no summary text was returned.';
        }
      },
      error: () => {
        this.summarizing = false;
        this.summaryInFlight = false;
        this.summaryError = 'Summary generation failed. Please try again.';
        this.snackbarService.error('Unable to summarize document.');
      }
    });
  }

  private revealWorkspace(): void {
    this.workspaceVisible = true;
    requestAnimationFrame(() => this.scrollToWorkspace());
  }

  private scrollToWorkspace(): void {
    requestAnimationFrame(() => {
      this.workspaceAnchor?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
  }

  formatFileSize(bytes: number): string {
    if (!Number.isFinite(bytes) || bytes < 0) return '—';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(bytes < 10 * 1024 ? 1 : 0)} KB`;
    if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(bytes < 10 * 1024 * 1024 ? 1 : 0)} MB`;
    return `${(bytes / (1024 * 1024 * 1024)).toFixed(1)} GB`;
  }

  goBack(): void {
    void this.router.navigate(['/documents']);
  }
}
