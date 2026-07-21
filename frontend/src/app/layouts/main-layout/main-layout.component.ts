import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';

import { AuthStateService } from '../../core/services/auth-state.service';
import { CurrentUserService } from '../../core/services/current-user.service';

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
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {

  fullName = '';

  constructor(
    private authStateService: AuthStateService,
    public currentUserService: CurrentUserService,
    private dialog: MatDialog
  ) { }


  logout(): void {
    this.dialog.open(ConfirmationDialogComponent,
      {
        width: '420px',
        data: {
          title: 'Logout',
          message: 'Are you sure you want to logout?',
          confirmText: 'Logout',
          cancelText: 'Stay'
        }
      })
      .afterClosed()
      .subscribe(result => {
        if (result) {
          this.authStateService.logout();
        }
      });

  }
}