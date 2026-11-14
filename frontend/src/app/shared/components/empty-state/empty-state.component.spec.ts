import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmptyStateComponent } from './empty-state.component';

describe('EmptyStateComponent', () => {
  let component: EmptyStateComponent;
  let fixture: ComponentFixture<EmptyStateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EmptyStateComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'No documents');
    fixture.componentRef.setInput('message', 'Upload a document to get started.');
    fixture.detectChanges();
  });

  it('renders the supplied title and message', () => {
    expect(fixture.nativeElement.textContent).toContain('No documents');
    expect(fixture.nativeElement.textContent).toContain('Upload a document to get started.');
  });

  it('uses the default icon when no icon is supplied', () => {
    expect(component.icon).toBe('inbox');
  });

  it('renders the action button only when action text is provided', () => {
    expect(fixture.nativeElement.querySelector('button')).toBeNull();

    fixture.componentRef.setInput('actionText', 'Upload');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('button')?.textContent).toContain('Upload');
  });

  it('emits the action event when the action button is clicked', () => {
    const action = vi.fn();
    component.action.subscribe(action);
    fixture.componentRef.setInput('actionText', 'Retry');
    fixture.detectChanges();

    fixture.nativeElement.querySelector('button').click();

    expect(action).toHaveBeenCalledOnce();
  });
});
