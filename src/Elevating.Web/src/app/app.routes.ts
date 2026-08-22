import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Sign in | Elevating',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./layout/auth-layout/auth-layout.component').then((module) => module.AuthLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/auth/login/login.component').then((module) => module.Login),
      },
    ],
  },
  {
    path: 'register',
    title: 'Get started | Elevating',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./layout/auth-layout/auth-layout.component').then((module) => module.AuthLayout),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/auth/register/register.component').then((module) => module.Register),
      },
    ],
  },
  {
    path: 'goals',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/app-shell/app-shell.component').then((module) => module.AppShell),
    children: [
      {
        path: '',
        pathMatch: 'full',
        title: 'Your goals | Elevating',
        loadComponent: () =>
          import('./features/goals/pages/goals-list/goals-list.component').then(
            (module) => module.GoalsList,
          ),
      },
      {
        path: 'new',
        title: 'New goal | Elevating',
        loadComponent: () =>
          import('./features/goals/pages/goal-form/goal-form.component').then(
            (module) => module.GoalForm,
          ),
      },
      {
        path: ':id/edit',
        title: 'Edit goal | Elevating',
        loadComponent: () =>
          import('./features/goals/pages/goal-form/goal-form.component').then(
            (module) => module.GoalForm,
          ),
      },
      {
        path: ':id',
        title: 'Goal | Elevating',
        loadComponent: () =>
          import('./features/goals/pages/goal-details/goal-details.component').then(
            (module) => module.GoalDetails,
          ),
      },
    ],
  },
  {
    path: '',
    loadComponent: () =>
      import('./layout/marketing-layout/marketing-layout.component').then(
        (module) => module.MarketingLayout,
      ),
    children: [
      {
        path: '',
        pathMatch: 'full',
        title: 'Elevating — Plan. Focus. Grow.',
        loadComponent: () =>
          import('./features/marketing/home/home.component').then((module) => module.Home),
      },
      {
        path: 'features',
        title: 'Features | Elevating',
        loadComponent: () =>
          import('./features/marketing/features/features.component').then(
            (module) => module.Features,
          ),
      },
      {
        path: 'about',
        title: 'About the project | Elevating',
        loadComponent: () =>
          import('./features/marketing/about/about.component').then((module) => module.About),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
