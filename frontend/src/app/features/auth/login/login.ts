import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LoginRequest } from '../../../core/models/login-request';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  hidePassword = true;
  loading = false;
  errorMessage = '';
  loginForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

 login(): void {

  if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    return;
  }

  const request: LoginRequest = {
    email: this.loginForm.value.email!,
    password: this.loginForm.value.password!
  };

  this.loading = true;
  this.errorMessage = '';

  this.authService.login(request)
    .pipe(
      finalize(() => this.loading = false)
    )
    .subscribe({

      next: () => {
        this.router.navigate(['/dashboard']);
      },

      error: (error) => {

        this.errorMessage =
          error.error?.message ??
          'Login failed. Please try again.';

      }

    });

}
}
