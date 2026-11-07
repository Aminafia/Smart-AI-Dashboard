import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageCardComponent } from './page-card.component';

describe('PageCardComponent', () => {
  let component: PageCardComponent;
  let fixture: ComponentFixture<PageCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageCardComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(PageCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates the shared page card container', () => {
    expect(component).toBeTruthy();
    expect(fixture.nativeElement.querySelector('mat-card')).toBeTruthy();
  });

  it('projects content into the card', () => {
    fixture.nativeElement.querySelector('mat-card').textContent = 'Card content';
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Card content');
  });
});
