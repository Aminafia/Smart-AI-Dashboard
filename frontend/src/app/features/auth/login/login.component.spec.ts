import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: { login: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    authService = { login: vi.fn() };
    router = { navigate: vi.fn().mockResolvedValue(true) };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates the login form with required email and password controls', () => {
    expect(component.loginForm).toBeTruthy();
    expect(component.loginForm.get('email')?.hasError('required')).toBe(true);
    expect(component.loginForm.get('password')?.hasError('required')).toBe(true);
  });

  it('rejects an invalid email address', () => {
    component.loginForm.setValue({ email: 'not-an-email', password: 'secret1' });

    component.login();

    expect(authService.login).not.toHaveBeenCalled();
    expect(component.loginForm.get('email')?.invalid).toBe(true);
  });

  it('rejects a password shorter than six characters', () => {
    component.loginForm.setValue({ email: 'amina@example.com', password: '12345' });

    component.login();

    expect(authService.login).not.toHaveBeenCalled();
  });

  it('submits valid credentials and navigates to the dashboard on success', () => {
    authService.login.mockReturnValue(of({ data: { token: 'jwt-token' } }));
    component.loginForm.setValue({ email: 'amina@example.com', password: 'secret1' });

    component.login();

    expect(authService.login).toHaveBeenCalledWith({
      email: 'amina@example.com',
      password: 'secret1'
    });
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('shows the API error message when login fails', () => {
    authService.login.mockReturnValue(
      throwError(() => ({ error: { message: 'Invalid credentials' } }))
    );
    component.loginForm.setValue({ email: 'amina@example.com', password: 'secret1' });

    component.login();

    expect(component.errorMessage).toBe('Invalid credentials');
  });

  it('shows a fallback message when the API error has no message', () => {
    authService.login.mockReturnValue(throwError(() => new Error('Network failure')));
    component.loginForm.setValue({ email: 'amina@example.com', password: 'secret1' });

    component.login();

    expect(component.errorMessage).toBe('Login failed. Please try again.');
  });

  it('clears a previous error before submitting a new request', () => {
    authService.login.mockReturnValue(of({ data: { token: 'jwt-token' } }));
    component.errorMessage = 'Previous failure';
    component.loginForm.setValue({ email: 'amina@example.com', password: 'secret1' });

    component.login();

    expect(component.errorMessage).toBe('');
  });
});
