import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AuthService } from './auth.service';

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.authStatus() === 'anonymous') {
    return true;
  }

  if (auth.authStatus() === 'authenticated') {
    return router.createUrlTree(['/goals']);
  }

  return auth
    .whenInitialized()
    .pipe(map((status) => (status === 'anonymous' ? true : router.createUrlTree(['/goals']))));
};
