import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { firstValueFrom, Observable, Subject } from 'rxjs';

import { AuthStatus } from './auth.models';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { guestGuard } from './guest.guard';

describe('authentication guards', () => {
  const status = signal<AuthStatus>('checking');
  let initialized: Subject<AuthStatus>;
  let router: Router;

  beforeEach(() => {
    status.set('checking');
    initialized = new Subject<AuthStatus>();

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            authStatus: status.asReadonly(),
            whenInitialized: () => initialized.asObservable(),
          },
        },
      ],
    });

    router = TestBed.inject(Router);
  });

  it('allows an authenticated user into a protected route', () => {
    status.set('authenticated');

    const result = runAuthGuard('/goals/42');

    expect(result).toBe(true);
  });

  it('redirects an anonymous user to login with the return URL', () => {
    status.set('anonymous');

    const result = runAuthGuard('/goals/42') as UrlTree;

    expect(router.serializeUrl(result)).toBe('/login?returnUrl=%2Fgoals%2F42');
  });

  it('waits while startup authentication is checking', async () => {
    const result = runAuthGuard('/goals/42') as Observable<boolean | UrlTree>;
    const resolved = firstValueFrom(result);
    let settled = false;
    void resolved.then(() => (settled = true));

    await Promise.resolve();
    expect(settled).toBe(false);

    initialized.next('authenticated');
    initialized.complete();

    expect(await resolved).toBe(true);
  });

  it('allows an anonymous user onto a guest route', () => {
    status.set('anonymous');

    const result = runGuestGuard();

    expect(result).toBe(true);
  });

  it('redirects an authenticated user away from guest routes', () => {
    status.set('authenticated');

    const result = runGuestGuard() as UrlTree;

    expect(router.serializeUrl(result)).toBe('/');
  });

  function runAuthGuard(url: string) {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );
  }

  function runGuestGuard() {
    return TestBed.runInInjectionContext(() =>
      guestGuard({} as ActivatedRouteSnapshot, { url: '/login' } as RouterStateSnapshot),
    );
  }
});
