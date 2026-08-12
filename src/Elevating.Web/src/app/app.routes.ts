import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((module) => module.Login),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then((module) => module.Register),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/goals/pages/goals-list/goals-list.component').then(
        (module) => module.GoalsList,
      ),
  },
  {
    path: 'goals/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/goals/pages/goal-form/goal-form.component').then(
        (module) => module.GoalForm,
      ),
  },
  {
    path: 'goals/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/goals/pages/goal-details/goal-details.component').then(
        (module) => module.GoalDetails,
      ),
  },
  {
    path: 'goals/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/goals/pages/goal-form/goal-form.component').then(
        (module) => module.GoalForm,
      ),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
