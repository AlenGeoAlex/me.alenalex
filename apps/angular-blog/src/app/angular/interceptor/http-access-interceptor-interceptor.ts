import {HttpErrorResponse, HttpInterceptorFn} from '@angular/common/http';

import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import {HotToastService} from '@ngxpert/hot-toast';

export const httpAccessInterceptorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastService = inject(HotToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        toastService.error("Your session has expired. Please log in again.");
        router.navigate(['/auth/google'])
          .catch(console.error);
      }

      return throwError(() => error);
    })
  );
};
