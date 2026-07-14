import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SummarizeComponent } from './summarize.component';

describe('SummarizeComponent', () => {
  let component: SummarizeComponent;
  let fixture: ComponentFixture<SummarizeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SummarizeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SummarizeComponent  );
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
