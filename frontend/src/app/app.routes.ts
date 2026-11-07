import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then(module => module.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then(module => module.RegisterComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    loadComponent: () =>
      import('./layouts/main-layout/main-layout.component').then(module => module.MainLayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(module => module.DashboardComponent)
      },
      {
        path: 'ai/generate',
        loadComponent: () =>
          import('./features/ai/generate/generate.component').then(module => module.GenerateComponent)
      },
      {
        path: 'summarize',
        loadComponent: () =>
          import('./features/ai/summarize/summarize.component').then(module => module.SummarizeComponent)
      },
      {
        path: 'ai/jobs',
        loadComponent: () =>
          import('./features/ai/job-history/job-history.component').then(module => module.JobHistoryComponent)
      },
      {
        path: 'documents',
        loadComponent: () =>
          import('./features/documents/pages/document-list/document-list.component').then(module => module.DocumentListComponent)
      },
      {
        path: 'documents/:id',
        loadComponent: () =>
          import('./features/documents/pages/document-details/document-details.component').then(module => module.DocumentDetailsComponent)
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];
