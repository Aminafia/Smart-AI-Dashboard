import { TestBed } from '@angular/core/testing';

import { DocumentService } from './document.service';

describe('Document', () => {
  let service: DocumentService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Document);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
