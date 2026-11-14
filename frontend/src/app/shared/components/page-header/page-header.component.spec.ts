import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageHeaderComponent } from './page-header.component';

describe('PageHeaderComponent', () => {
  let component: PageHeaderComponent;
  let fixture: ComponentFixture<PageHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageHeaderComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PageHeaderComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('title', 'Documents');
    fixture.detectChanges();
  });

  it('renders its required title', () => {
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Documents');
  });

  it('does not render a subtitle when it is empty', () => {
    expect(fixture.nativeElement.querySelector('p')).toBeNull();
  });

  it('renders a supplied subtitle', () => {
    fixture.componentRef.setInput('subtitle', 'Manage your uploaded files');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('p')?.textContent).toContain(
      'Manage your uploaded files',
    );
  });

  it('provides a dedicated action projection container', () => {
    expect(fixture.nativeElement.querySelector('.page-header-actions')).toBeTruthy();
  });
});
