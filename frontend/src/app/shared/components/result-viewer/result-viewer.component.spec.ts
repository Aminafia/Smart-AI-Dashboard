import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResultViewerComponent } from './result-viewer.component';

describe('ResultViewerComponent', () => {
  let component: ResultViewerComponent;
  let fixture: ComponentFixture<ResultViewerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResultViewerComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ResultViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates with default empty-state values', () => {
    expect(component).toBeTruthy();
    expect(component.emptyTitle).toBe('No Results Yet');
    expect(component.emptyMessage).toBe('Run an AI operation to see the result here.');
  });

  it('renders supplied result HTML', () => {
    fixture.componentRef.setInput('html', '<p>Generated result</p>');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Generated result');
  });

  it('renders a supplied error message', () => {
    fixture.componentRef.setInput('error', 'Generation failed');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Generation failed');
  });
});
