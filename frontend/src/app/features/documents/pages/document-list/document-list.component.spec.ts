import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { DocumentService } from '../../../../core/services/document.service';
import { SnackbarService } from '../../../../shared/services/snackbar.service';
import { DocumentListComponent } from './document-list.component';

describe('DocumentListComponent', () => {
  let component: DocumentListComponent;
  let fixture: ComponentFixture<DocumentListComponent>;
  let documentService: { getDocuments: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    documentService = {
      getDocuments: vi.fn().mockReturnValue(of({ items: [], totalCount: 0 }))
    };
    router = { navigate: vi.fn().mockResolvedValue(true) };

    await TestBed.configureTestingModule({
      imports: [DocumentListComponent],
      providers: [
        { provide: DocumentService, useValue: documentService },
        { provide: SnackbarService, useValue: { success: vi.fn(), error: vi.fn() } },
        { provide: MatDialog, useValue: { open: vi.fn() } },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads the first page of documents on initialization', () => {
    expect(documentService.getDocuments).toHaveBeenCalledWith(1, 10);
    expect(component.documents).toEqual([]);
    expect(component.totalCount).toBe(0);
  });

  it('navigates to the selected document details route', () => {
    const document = { id: 'doc-1', fileName: 'report.pdf' } as never;

    component.viewDocument(document);

    expect(router.navigate).toHaveBeenCalledWith(['/documents', 'doc-1']);
  });

  it('updates pagination and reloads the document list', () => {
    component.onPageChange({ pageIndex: 2, pageSize: 25, length: 80 });

    expect(component.page).toBe(3);
    expect(component.pageSize).toBe(25);
    expect(documentService.getDocuments).toHaveBeenLastCalledWith(3, 25);
  });

  it('formats zero-byte and kilobyte values', () => {
    expect(component.formatFileSize(0)).toBe('0 Bytes');
    expect(component.formatFileSize(1024)).toBe('1.00 KB');
  });

  it('maps known content types to file extensions', () => {
    expect(component.getFileType('application/pdf')).toBe('PDF');
    expect(component.getFileType('text/plain')).toBe('TXT');
    expect(component.getFileType('application/octet-stream')).toBe('application/octet-stream');
  });
});
