import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { ConfirmationDialogComponent } from './confirmation-dialog.component';

describe('ConfirmationDialogComponent', () => {
  let component: ConfirmationDialogComponent;
  let fixture: ComponentFixture<ConfirmationDialogComponent>;
  let dialogRef: { close: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    dialogRef = { close: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [ConfirmationDialogComponent],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            title: 'Delete document',
            message: 'Are you sure?',
            confirmText: 'Delete',
            cancelText: 'Cancel',
          },
        },
        { provide: MatDialogRef, useValue: dialogRef },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmationDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders the supplied confirmation content', () => {
    expect(fixture.nativeElement.textContent).toContain('Delete document');
    expect(fixture.nativeElement.textContent).toContain('Are you sure?');
  });

  it('closes with true when confirmed', () => {
    component.confirm();

    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('closes with false when cancelled', () => {
    component.cancel();

    expect(dialogRef.close).toHaveBeenCalledWith(false);
  });
});
