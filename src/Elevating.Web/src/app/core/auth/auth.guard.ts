import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.authStatus() === 'authenticated') {
    return true;
  }

  if (auth.authStatus() === 'anonymous') {
    return router.createUrlTree(['/login'], {
      queryParams: {
        returnUrl: state.url,
      },
    });
  }

  return auth.whenInitialized().pipe(
    map((status) =>
      status === 'authenticated'
        ? true
        : router.createUrlTree(['/login'], {
            queryParams: {
              returnUrl: state.url,
            },
          }),
    ),
  );
};
