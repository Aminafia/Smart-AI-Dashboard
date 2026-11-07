import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';

import { AuthStateService } from '../../core/services/auth-state.service';
import { CurrentUserService } from '../../core/services/current-user.service';
import { MainLayoutComponent } from './main-layout.component';

describe('MainLayoutComponent', () => {
  let component: MainLayoutComponent;
  let fixture: ComponentFixture<MainLayoutComponent>;
  let authState: { logout: ReturnType<typeof vi.fn> };
  let dialog: { open: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authState = { logout: vi.fn() };
    dialog = { open: vi.fn().mockReturnValue({ afterClosed: () => of(false) }) };

    await TestBed.configureTestingModule({
      imports: [MainLayoutComponent],
      providers: [
        provideRouter([]),
        { provide: AuthStateService, useValue: authState },
        { provide: CurrentUserService, useValue: { fullName: () => 'Amina' } },
        { provide: MatDialog, useValue: dialog }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates the authenticated application layout', () => {
    expect(component).toBeTruthy();
  });

  it('shows the signed-in user and primary workspace navigation', () => {
    expect(fixture.nativeElement.textContent).toContain('Amina');
    expect(fixture.nativeElement.textContent).toContain('Dashboard');
    expect(fixture.nativeElement.textContent).toContain('Documents');
  });

  it('opens the logout confirmation dialog', () => {
    component.logout();

    expect(dialog.open).toHaveBeenCalledOnce();
    expect(authState.logout).not.toHaveBeenCalled();
  });
});
