import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobHistoryComponent } from './job-history.component';
describe('JobHistory', () => {
  let component: JobHistoryComponent;
  let fixture: ComponentFixture<JobHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobHistoryComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobHistoryComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
