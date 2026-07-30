import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentListComponent } from './document-list.component';

describe('DocumentList', () => {
  let component: DocumentListComponent;
  let fixture: ComponentFixture<DocumentListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocumentListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DocumentListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
