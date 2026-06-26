import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Generate } from './generate';

describe('Generate', () => {
  let component: Generate;
  let fixture: ComponentFixture<Generate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Generate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Generate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
