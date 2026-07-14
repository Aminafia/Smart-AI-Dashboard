import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { catchError, throwError } from 'rxjs';

import { SnackbarService } from '../../shared/services/snackbar.service';
import { AuthStateService } from '../services/auth-state.service';

function getMessage(error: HttpErrorResponse): string {

  const apiError = error.error;

  if (!apiError) {
    return 'Something went wrong.';
  }

  return (
    apiError.message ??
    apiError.Message ??
    'Something went wrong.'
  );

}

function getErrors(error: HttpErrorResponse): string[] {

  const apiError = error.error;

  if (!apiError) {
    return [];
  }

  return (
    apiError.errors ??
    apiError.Errors ??
    []
  );

}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {

  const snackbar = inject(SnackbarService);
  const authStateService = inject(AuthStateService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {

      switch (error.status) {

        case 0:
          snackbar.error('Unable to connect to the server.');
          break;

        case 400:
          {
            const errors = getErrors(error);
            if (errors.length > 0) {
              snackbar.warning(errors);  }
            else {
              snackbar.warning(getMessage(error));  }
          break;
          }

        case 401:
          snackbar.error(getMessage(error));
          authStateService.logout();
          break;

        case 403:
          snackbar.error(getMessage(error));
          break;

        case 404:
          snackbar.warning(getMessage(error));
          break;

        case 500:
          snackbar.error(getMessage(error));
          break;

        default:
          snackbar.error(getMessage(error));
      }

      return throwError(() => error);

    })
  );

};