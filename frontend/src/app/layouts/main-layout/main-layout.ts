import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';

import { AuthStateService } from '../../core/services/auth-state.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    RouterLink,

    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css'
})
export class MainLayout {
  constructor(
    private authStateService: AuthStateService,
  ) {}
  
  logout(): void {

    this.authStateService.logout();
  }
}