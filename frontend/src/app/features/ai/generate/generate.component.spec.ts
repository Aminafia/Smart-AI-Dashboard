import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenerateComponent } from './generate.component';

describe('Generate', () => {
  let component: GenerateComponent;
  let fixture: ComponentFixture<GenerateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenerateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GenerateComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
