import { Injectable } from '@angular/core';

import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {

  private readonly duration = 3000;

  constructor(
    private snackBar: MatSnackBar
  ) {}

  success(message: string): void {

    this.snackBar.open(message, 'Close', {
      duration: this.duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['snackbar-success']
    });

  }

  error(message: string): void {

    this.snackBar.open(message, 'Close', {
      duration: this.duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['snackbar-error']
    });

  }

  warning(message: string): void {

    this.snackBar.open(message, 'Close', {
      duration: this.duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['snackbar-warning']
    });

  }

  info(message: string): void {

    this.snackBar.open(message, 'Close', {
      duration: this.duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['snackbar-info']
    });

  }

}