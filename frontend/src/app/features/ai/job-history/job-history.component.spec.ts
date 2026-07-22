import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobHistory } from './job-history';

describe('JobHistory', () => {
  let component: JobHistory;
  let fixture: ComponentFixture<JobHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobHistory]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobHistory);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
