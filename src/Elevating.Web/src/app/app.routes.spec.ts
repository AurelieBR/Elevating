import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';
import { AuthLayout } from './layout/auth-layout/auth-layout.component';
import { routes } from './app.routes';

describe('application routes', () => {
  it('keeps auth routes guest-only with branded titles and the shared AuthLayout', async () => {
    const login = routes.find((route) => route.path === 'login');
    const register = routes.find((route) => route.path === 'register');

    expect(login?.canActivate).toContain(guestGuard);
    expect(login?.title).toBe('Sign in | Elevating');
    expect(login?.children?.map((route) => route.path)).toEqual(['']);
    expect(register?.canActivate).toContain(guestGuard);
    expect(register?.title).toBe('Get started | Elevating');
    expect(register?.children?.map((route) => route.path)).toEqual(['']);

    expect(await login?.loadComponent?.()).toBe(AuthLayout);
    expect(await register?.loadComponent?.()).toBe(AuthLayout);
  });

  it('groups every protected Goal page beneath /goals and AppShell', () => {
    const goals = routes.find((route) => route.path === 'goals');

    expect(goals?.canActivate).toContain(authGuard);
    expect(goals?.children?.map((route) => route.path)).toEqual(['', 'new', ':id/edit', ':id']);
    expect(goals?.children?.find((route) => route.path === '')?.title).toBe(
      'Your goals | Elevating',
    );
  });

  it('groups the public journey beneath MarketingLayout without Pricing', () => {
    const marketing = routes.find((route) => route.path === '');

    expect(marketing?.canActivate).toBeUndefined();
    expect(marketing?.children?.map((route) => route.path)).toEqual(['', 'features', 'about']);
    expect(marketing?.children?.map((route) => route.title)).toEqual([
      'Elevating — Plan. Focus. Grow.',
      'Features | Elevating',
      'About the project | Elevating',
    ]);
    expect(marketing?.children?.some((route) => route.path === 'pricing')).toBe(false);
  });

  it('returns unknown routes to the public homepage', () => {
    expect(routes.at(-1)).toEqual(
      expect.objectContaining({
        path: '**',
        redirectTo: '',
      }),
    );
  });
});
