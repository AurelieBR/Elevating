import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CreateGoalRequest, GoalPriority, GoalStatus, UpdateGoalRequest } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';

interface GoalFormControls {
  title: FormControl<string>;
  category: FormControl<string>;
  description: FormControl<string>;
  priority: FormControl<GoalPriority>;
  status: FormControl<GoalStatus>;
  targetDate: FormControl<string>;
}

interface ValidationProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

@Component({
  selector: 'app-goal-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './goal-form.component.html',
  styleUrl: './goal-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalForm {
  private readonly goalsApi = inject(GoalsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected goalId: number | null = null;

  readonly isEditMode = signal(false);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly submitError = signal<string | null>(null);

  readonly pageTitle = computed(() => (this.isEditMode() ? 'Edit goal' : 'Create a new goal'));

  readonly pageDescription = computed(() =>
    this.isEditMode()
      ? 'Update the details and progress of your goal.'
      : 'Turn your next ambition into a clear and actionable goal.',
  );

  readonly priorities = [
    {
      value: GoalPriority.Low,
      label: 'Low',
      description: 'Can be completed when time allows.',
    },
    {
      value: GoalPriority.Medium,
      label: 'Medium',
      description: 'Important, but not immediately urgent.',
    },
    {
      value: GoalPriority.High,
      label: 'High',
      description: 'Needs your attention and focused effort.',
    },
  ];

  readonly statuses = [
    {
      value: GoalStatus.NotStarted,
      label: 'Not started',
    },
    {
      value: GoalStatus.InProgress,
      label: 'In progress',
    },
    {
      value: GoalStatus.Completed,
      label: 'Completed',
    },
  ];

  readonly form = new FormGroup<GoalFormControls>({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(200)],
    }),
    category: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(2000)],
    }),
    priority: new FormControl(GoalPriority.Medium, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    status: new FormControl(GoalStatus.NotStarted, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    targetDate: new FormControl('', {
      nonNullable: true,
    }),
  });

  constructor() {
    this.initializeFromRoute();
  }

  submit(): void {
    this.submitError.set(null);

    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.isEditMode()) {
      this.updateGoal();
      return;
    }

    this.createGoal();
  }

  retryLoad(): void {
    if (this.goalId !== null) {
      this.loadGoal(this.goalId);
    }
  }

  titleError(): string | null {
    const control = this.form.controls.title;

    if (!control.touched) {
      return null;
    }

    if (control.hasError('required')) {
      return 'A title is required.';
    }

    if (control.hasError('maxlength')) {
      return 'The title cannot exceed 200 characters.';
    }

    return null;
  }

  categoryError(): string | null {
    const control = this.form.controls.category;

    if (!control.touched) {
      return null;
    }

    if (control.hasError('required')) {
      return 'A category is required.';
    }

    if (control.hasError('maxlength')) {
      return 'The category cannot exceed 100 characters.';
    }

    return null;
  }

  descriptionError(): string | null {
    const control = this.form.controls.description;

    if (control.touched && control.hasError('maxlength')) {
      return 'The description cannot exceed 2,000 characters.';
    }

    return null;
  }

  private initializeFromRoute(): void {
    const idValue = this.route.snapshot.paramMap.get('id');

    if (idValue === null) {
      return;
    }

    const parsedId = Number(idValue);

    if (!Number.isInteger(parsedId) || parsedId <= 0) {
      this.isEditMode.set(true);
      this.loadError.set('The requested goal ID is invalid.');
      return;
    }

    this.goalId = parsedId;
    this.isEditMode.set(true);
    this.loadGoal(parsedId);
  }

  private loadGoal(id: number): void {
    this.loading.set(true);
    this.loadError.set(null);

    this.goalsApi
      .getById(id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe({
        next: (goal) => {
          this.form.setValue({
            title: goal.title,
            category: goal.category,
            description: goal.description ?? '',
            priority: goal.priority,
            status: goal.status,
            targetDate: this.toDateInputValue(goal.targetDate),
          });

          this.form.markAsPristine();
        },
        error: (error: HttpErrorResponse) => {
          this.loadError.set(
            error.status === 404
              ? 'The requested goal could not be found.'
              : 'We could not load this goal. Make sure the API is running and try again.',
          );
        },
      });
  }

  private createGoal(): void {
    const value = this.form.getRawValue();

    const request: CreateGoalRequest = {
      title: value.title.trim(),
      category: value.category.trim(),
      description: this.toNullableText(value.description),
      priority: value.priority,
      targetDate: this.toNullableDate(value.targetDate),
    };

    this.saving.set(true);

    this.goalsApi
      .create(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.saving.set(false);
        }),
      )
      .subscribe({
        next: (createdGoal) => {
          void this.router.navigate(['/goals', createdGoal.id], {
            queryParams: {
              notice: 'created',
            },
          });
        },
        error: (error: HttpErrorResponse) => {
          this.submitError.set(this.getSubmitError(error));
        },
      });
  }

  private updateGoal(): void {
    if (this.goalId === null) {
      this.submitError.set('The goal cannot be updated because its ID is missing.');
      return;
    }

    const value = this.form.getRawValue();

    const request: UpdateGoalRequest = {
      title: value.title.trim(),
      category: value.category.trim(),
      description: this.toNullableText(value.description),
      priority: value.priority,
      status: value.status,
      targetDate: this.toNullableDate(value.targetDate),
    };

    this.saving.set(true);

    this.goalsApi
      .update(this.goalId, request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.saving.set(false);
        }),
      )
      .subscribe({
        next: () => {
          void this.router.navigate(['/goals', this.goalId], {
            queryParams: {
              notice: 'updated',
            },
          });
        },
        error: (error: HttpErrorResponse) => {
          this.submitError.set(
            error.status === 404 ? 'This goal no longer exists.' : this.getSubmitError(error),
          );
        },
      });
  }

  private getSubmitError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'The API could not be reached. Make sure it is running and try again.';
    }

    const problem = error.error as ValidationProblemDetails | undefined;

    if (problem?.errors) {
      const firstError = Object.values(problem.errors)
        .flat()
        .find((message) => Boolean(message));

      if (firstError) {
        return firstError;
      }
    }

    return (
      problem?.detail ??
      problem?.title ??
      'The goal could not be saved. Please review the form and try again.'
    );
  }

  private toNullableText(value: string): string | null {
    const trimmedValue = value.trim();

    return trimmedValue.length > 0 ? trimmedValue : null;
  }

  private toNullableDate(value: string): string | null {
    return value.length > 0 ? value : null;
  }

  private toDateInputValue(value: string | null): string {
    return value?.slice(0, 10) ?? '';
  }
}
