import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/goals/pages/goals-list/goals-list.component').then(
        (module) => module.GoalsList,
      ),
  },
  {
    path: 'goals/new',
    loadComponent: () =>
      import('./features/goals/pages/goal-form/goal-form.component').then(
        (module) => module.GoalForm,
      ),
  },
  {
    path: 'goals/:id/edit',
    loadComponent: () =>
      import('./features/goals/pages/goal-form/goal-form.component').then(
        (module) => module.GoalForm,
      ),
  },
  {
    path: 'goals/:id',
    loadComponent: () =>
      import('./features/goals/pages/goal-details/goal-details.component').then(
        (module) => module.GoalDetails,
      ),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
