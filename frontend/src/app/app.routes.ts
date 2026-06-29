import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { Login } from './features/auth/login/login'; 
import { MainLayout } from './layouts/main-layout/main-layout';
import { Dashboard } from './features/dashboard/dashboard';
import { Generate } from './features/ai/generate/generate';

export const routes: Routes = [
  {
    path: 'login',
    component: Login
  },
  {
    path: '',
    component: MainLayout,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: Dashboard },
      { path: 'ai/generate', component: Generate },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];