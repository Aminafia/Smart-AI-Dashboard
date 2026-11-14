import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';

import { SnackbarService } from './snackbar.service';

describe('SnackbarService', () => {
  let service: SnackbarService;
  let snackBar: { open: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    snackBar = { open: vi.fn() };
    TestBed.configureTestingModule({
      providers: [SnackbarService, { provide: MatSnackBar, useValue: snackBar }],
    });
    service = TestBed.inject(SnackbarService);
  });

  it('shows success messages with the success style', () => {
    service.success('Saved');

    expect(snackBar.open).toHaveBeenCalledWith(
      'Saved',
      'Close',
      expect.objectContaining({
        duration: 3000,
        panelClass: ['snackbar-success'],
      }),
    );
  });

  it('shows error messages with the error style', () => {
    service.error('Failed');

    expect(snackBar.open).toHaveBeenCalledWith(
      'Failed',
      'Close',
      expect.objectContaining({
        panelClass: ['snackbar-error'],
      }),
    );
  });

  it('joins validation message arrays before displaying them', () => {
    service.warning(['First error', 'Second error']);

    expect(snackBar.open).toHaveBeenCalledWith(
      'First error\nSecond error',
      'Close',
      expect.objectContaining({
        panelClass: ['snackbar-warning'],
      }),
    );
  });

  it('shows string warnings without changing their content', () => {
    service.warning('Check your input');

    expect(snackBar.open).toHaveBeenCalledWith(
      'Check your input',
      'Close',
      expect.objectContaining({
        panelClass: ['snackbar-warning'],
      }),
    );
  });

  it('shows informational messages with the info style', () => {
    service.info('Processing');

    expect(snackBar.open).toHaveBeenCalledWith(
      'Processing',
      'Close',
      expect.objectContaining({
        panelClass: ['snackbar-info'],
      }),
    );
  });
});
