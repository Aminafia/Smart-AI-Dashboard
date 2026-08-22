import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentDetails } from './document-details.component';

describe('DocumentDetails', () => {
  let component: DocumentDetails;
  let fixture: ComponentFixture<DocumentDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocumentDetails]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DocumentDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
