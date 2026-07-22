import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

import { LoginComponent } from './features/auth/login/login.component';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { GenerateComponent } from './features/ai/generate/generate.component';
import { SummarizeComponent } from './features/ai/summarize/summarize.component';
import { JobHistoryComponent } from './features/ai/job-history/job-history.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard]
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'ai/generate', component: GenerateComponent },
      { path: 'summarize', component: SummarizeComponent },
      { path: 'ai/jobs', component: JobHistoryComponent },
      //Add paths here
      
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];