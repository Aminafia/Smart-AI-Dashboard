import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkspaceSectionComponent } from './workspace-section.component';

describe('WorkspaceSectionComponent', () => {
  let component: WorkspaceSectionComponent;
  let fixture: ComponentFixture<WorkspaceSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WorkspaceSectionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WorkspaceSectionComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
