import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';

import { AiService } from '../../../../core/services/ai.service';
import { DocumentService } from '../../../../core/services/document.service';
import { SnackbarService } from '../../../../shared/services/snackbar.service';
import { DocumentDetailsComponent } from './document-details.component';

describe('DocumentDetailsComponent', () => {
  let component: DocumentDetailsComponent;
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    router = { navigate: vi.fn().mockResolvedValue(true) };

    await TestBed.configureTestingModule({
      imports: [DocumentDetailsComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: vi.fn().mockReturnValue('doc-1') } } }
        },
        { provide: Router, useValue: router },
        { provide: DocumentService, useValue: {} },
        { provide: AiService, useValue: {} },
        { provide: SnackbarService, useValue: { error: vi.fn(), success: vi.fn() } }
      ]
    }).compileComponents();

    component = TestBed.createComponent(DocumentDetailsComponent).componentInstance;
  });

  it('formats byte values without converting them to KB', () => {
    expect(component.formatFileSize(0)).toBe('0 B');
    expect(component.formatFileSize(512)).toBe('512 B');
  });

  it('formats kilobytes with one decimal below ten and no decimal above ten', () => {
    expect(component.formatFileSize(1536)).toBe('1.5 KB');
    expect(component.formatFileSize(12 * 1024)).toBe('12 KB');
  });

  it('formats megabytes and gigabytes', () => {
    expect(component.formatFileSize(1.5 * 1024 * 1024)).toBe('1.5 MB');
    expect(component.formatFileSize(2 * 1024 * 1024 * 1024)).toBe('2.0 GB');
  });

  it('returns an em dash for negative or non-finite file sizes', () => {
    expect(component.formatFileSize(-1)).toBe('—');
    expect(component.formatFileSize(Number.NaN)).toBe('—');
    expect(component.formatFileSize(Number.POSITIVE_INFINITY)).toBe('—');
  });

  it('toggles PDF preview visibility', () => {
    expect(component.previewVisible).toBe(true);

    component.togglePreview();
    expect(component.previewVisible).toBe(false);

    component.togglePreview();
    expect(component.previewVisible).toBe(true);
  });

  it('redirects to the document list when the route has no document ID', () => {
    const route = TestBed.inject(ActivatedRoute);
    vi.spyOn(route.snapshot.paramMap, 'get').mockReturnValue(null);

    component.ngOnInit();

    expect(router.navigate).toHaveBeenCalledWith(['/documents']);
  });

  it('navigates back to the document list', () => {
    component.goBack();

    expect(router.navigate).toHaveBeenCalledWith(['/documents']);
  });
});
