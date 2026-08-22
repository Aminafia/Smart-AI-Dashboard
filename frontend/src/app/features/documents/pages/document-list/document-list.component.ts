import { Component, OnInit } from '@angular/core';

import { DocumentModel } from '../../../../core/models/documents/document.model';
import { DocumentService } from '../../../../core/services/document.service';
import { SnackbarService } from '../../../../shared/services/snackbar.service';

import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { PageCardComponent } from '../../../../shared/components/page-card/page-card.component';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Router } from '@angular/router';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    PageHeaderComponent,
    PageCardComponent,
    EmptyStateComponent,
    MatDialogModule,
    MatPaginatorModule
  ],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.css'
})
export class DocumentListComponent implements OnInit {

  documents: DocumentModel[] = [];

  page = 1;
  pageSize = 10;
  totalCount = 0;
  displayedColumns = [
    'fileName',
    'contentType',
    'fileSize',
    'uploadedAt',
    'actions'
  ];

  constructor(
    private readonly documentService: DocumentService,
    private readonly snackbarService: SnackbarService,
    private readonly dialog: MatDialog,
    private readonly router: Router

  ) { }

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.documentService
      .getDocuments(this.page, this.pageSize)
      .subscribe(response => {
        this.documents = response.items;
        this.totalCount = response.totalCount;
      });
  }

  uploadDocument(event: Event): void {

    const input = event.target as HTMLInputElement;
    if (!input.files?.length) {
      return;
    }

    const file = input.files[0];

    this.documentService
      .upload(file)
      .subscribe({
        next: () => {
          this.snackbarService.success('Document uploaded successfully.');
          this.loadDocuments();
          input.value = '';
        },
        error: () => {
          input.value = '';
        }
      });
  }

  viewDocument(documentModel: DocumentModel): void {
    this.router.navigate([
      '/documents',
      documentModel.id
    ]);
  }

  downloadDocument(documentModel: DocumentModel): void {

    this.documentService
      .download(documentModel.id)
      .subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = documentModel.fileName;
        link.click();
        window.URL.revokeObjectURL(url);

      });
  }

  deleteDocument(documentModel: DocumentModel): void {

    this.dialog.open(ConfirmationDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete Document',
        message: `Are you sure you want to delete "${documentModel.fileName}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    })
      .afterClosed()
      .subscribe(result => {
        if (!result) {
          return;
        }
        this.documentService
          .delete(documentModel.id)
          .subscribe({
            next: () => {
              this.snackbarService.success('Document deleted successfully.');
              this.loadDocuments();
            }
          });
      });

  }

  formatFileSize(bytes: number): string {

    if (bytes === 0) {
      return '0 Bytes';
    }
    const kb = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const index = Math.floor(Math.log(bytes) / Math.log(kb));

    return `${(bytes / Math.pow(kb, index)).toFixed(2)} ${sizes[index]}`;

  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadDocuments();
  }

  getFileType(contentType: string): string {
    switch (contentType) {
      case 'application/pdf': return 'PDF';
      case 'application/msword': return 'DOC';
      case 'application/vnd.openxmlformats-officedocument.wordprocessingml.document': return 'DOCX';
      case 'text/plain': return 'TXT';
      default: return contentType;
    }
  }

}