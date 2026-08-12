import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

const retriedAfterRefresh = new HttpContextToken<boolean>(() => false);
const apiBaseUrl = environment.apiBaseUrl.replace(/\/+$/, '');
const cookieAuthUrls = new Set([
  `${apiBaseUrl}/auth/register`,
  `${apiBaseUrl}/auth/login`,
  `${apiBaseUrl}/auth/refresh`,
  `${apiBaseUrl}/auth/logout`,
]);

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (!isElevatingApiRequest(request.url)) {
    return next(request);
  }

  const auth = inject(AuthService);
  const router = inject(Router);
  const requestUrl = request.url.split('?')[0];
  const isCookieAuthRequest = cookieAuthUrls.has(requestUrl);
  const token = auth.accessToken();

  let preparedRequest = isCookieAuthRequest ? request.clone({ withCredentials: true }) : request;

  if (token !== null && !isCookieAuthRequest) {
    preparedRequest = preparedRequest.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(preparedRequest).pipe(
    catchError((error: unknown) => {
      if (
        !(error instanceof HttpErrorResponse) ||
        error.status !== 401 ||
        isCookieAuthRequest ||
        preparedRequest.context.get(retriedAfterRefresh)
      ) {
        return throwError(() => error);
      }

      return auth.refresh().pipe(
        catchError((refreshError: unknown) => {
          auth.clearSession();

          void router.navigate(['/login'], {
            queryParams: {
              returnUrl: router.url,
            },
          });

          return throwError(() => refreshError);
        }),
        switchMap((freshToken) =>
          next(
            preparedRequest.clone({
              context: preparedRequest.context.set(retriedAfterRefresh, true),
              setHeaders: {
                Authorization: `Bearer ${freshToken}`,
              },
            }),
          ),
        ),
      );
    }),
  );
};

function isElevatingApiRequest(url: string): boolean {
  return url === apiBaseUrl || url.startsWith(`${apiBaseUrl}/`);
}
