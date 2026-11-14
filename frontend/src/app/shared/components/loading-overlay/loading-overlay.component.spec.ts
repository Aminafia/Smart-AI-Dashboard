import { TestBed } from '@angular/core/testing';

import { LoadingService } from '../../../core/services/loading.service';
import { LoadingOverlayComponent } from './loading-overlay.component';

describe('LoadingOverlayComponent', () => {
  it('is hidden while the application is idle', () => {
    TestBed.configureTestingModule({ imports: [LoadingOverlayComponent] });
    const fixture = TestBed.createComponent(LoadingOverlayComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.loading-overlay')).toBeNull();
  });

  it('appears while a request is active and disappears after completion', () => {
    TestBed.configureTestingModule({ imports: [LoadingOverlayComponent] });
    const fixture = TestBed.createComponent(LoadingOverlayComponent);
    const loadingService = TestBed.inject(LoadingService);

    loadingService.show();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.loading-overlay')).toBeTruthy();

    loadingService.hide();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.loading-overlay')).toBeNull();
  });
});
