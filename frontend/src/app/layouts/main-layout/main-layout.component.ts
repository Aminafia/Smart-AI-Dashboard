import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthStateService } from '../../core/services/auth-state.service';
import { CurrentUserService } from '../../core/services/current-user.service';
import { ConfirmationDialogComponent } from '../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,

    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css',
})
export class MainLayoutComponent {
  constructor(
    private authStateService: AuthStateService,
    public currentUserService: CurrentUserService,
    private dialog: MatDialog,
  ) {}

  logout(): void {
    this.dialog
      .open(ConfirmationDialogComponent, {
        width: '420px',
        data: {
          title: 'Logout',
          message: 'Are you sure you want to logout?',
          confirmText: 'Logout',
          cancelText: 'Stay',
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.authStateService.logout();
        }
      });
  }
}
