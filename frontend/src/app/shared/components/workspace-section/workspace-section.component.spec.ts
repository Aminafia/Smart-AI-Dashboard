import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkspaceSectionComponent } from './workspace-section.component';

describe('WorkspaceSectionComponent', () => {
  let component: WorkspaceSectionComponent;
  let fixture: ComponentFixture<WorkspaceSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkspaceSectionComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(WorkspaceSectionComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'Generation workspace');
    fixture.detectChanges();
  });

  it('renders its required title', () => {
    expect(fixture.nativeElement.textContent).toContain('Generation workspace');
  });

  it('omits an empty subtitle', () => {
    expect(fixture.nativeElement.querySelector('p')).toBeNull();
  });

  it('renders a supplied subtitle', () => {
    fixture.componentRef.setInput('subtitle', 'Create AI-generated content');
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Create AI-generated content');
  });

  it('renders the shared page card container', () => {
    expect(fixture.nativeElement.querySelector('app-page-card')).toBeTruthy();
  });
});
