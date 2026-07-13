import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs';

import { LoadingService } from '../services/loading.service';
import { SKIP_LOADING } from '../constants/loading-context';

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {

  if (req.context.get(SKIP_LOADING)) {
    return next(req);
  }
  
  const loadingService = inject(LoadingService);

  loadingService.show();

  return next(req).pipe(
    finalize(() => loadingService.hide())
  );

};