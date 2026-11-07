import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { register: vi.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates the registration form', () => {
    expect(component.registerForm).toBeTruthy();
  });

  it('requires a name, valid email, and a password of at least six characters', () => {
    expect(component.registerForm.invalid).toBe(true);

    component.registerForm.setValue({
      name: 'Amina',
      email: 'amina@example.com',
      password: 'secret1'
    });

    expect(component.registerForm.valid).toBe(true);
  });
});
