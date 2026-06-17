import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';


export const jwtInterceptor: HttpInterceptorFn = (req: HttpRequest<any>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getAccessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('refresh-token')) {
        const refreshToken = authService.getRefreshToken() || '';
        return authService.refreshAccessToken(refreshToken).pipe(
          switchMap((res: any) => {
            authService.saveTokens(res.token, res.refreshToken);
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${res.token}` }
            });
            return next(retryReq);
          }),
          catchError((refreshError) => {
            authService.clearTokens();
            const currentUrl = router.url;
            router.navigate(['/auth/login'], { queryParams: { returnUrl: currentUrl } });
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};