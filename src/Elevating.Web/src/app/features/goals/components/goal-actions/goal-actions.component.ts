import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { Goal, GoalAction, GoalActionStatus } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';

@Component({
  selector: 'app-goal-actions',
  imports: [ReactiveFormsModule],
  templateUrl: './goal-actions.component.html',
  styleUrl: './goal-actions.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalActions implements OnInit {
  private readonly goalsApi = inject(GoalsApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly goal = input.required<Goal>();

  readonly goalChanged = output<void>();

  readonly actions = signal<GoalAction[]>([]);
  readonly loading = signal(true);
  readonly adding = signal(false);
  readonly processingActionId = signal<number | null>(null);
  readonly editingActionId = signal<number | null>(null);
  readonly errorMessage = signal<string | null>(null);

  readonly newActionTitle = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(200)],
  });

  readonly editActionTitle = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(200)],
  });

  readonly GoalActionStatus = GoalActionStatus;

  ngOnInit(): void {
    this.loadActions();
  }

  addAction(): void {
    const title = this.newActionTitle.value.trim();

    if (!title || this.adding()) {
      this.newActionTitle.markAsTouched();
      return;
    }

    this.adding.set(true);
    this.errorMessage.set(null);

    this.goalsApi
      .createAction(this.goal().id, { title })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.adding.set(false)),
      )
      .subscribe({
        next: () => {
          this.newActionTitle.reset('');
          this.refreshAfterChange();
        },
        error: () => {
          this.errorMessage.set('The action could not be added.');
        },
      });
  }

  toggleAction(action: GoalAction): void {
    if (this.processingActionId() !== null) {
      return;
    }

    this.processingActionId.set(action.id);
    this.errorMessage.set(null);

    const request =
      action.status === GoalActionStatus.Completed
        ? this.goalsApi.reopenAction(action.goalId, action.id)
        : this.goalsApi.completeAction(action.goalId, action.id);

    request
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.processingActionId.set(null)),
      )
      .subscribe({
        next: () => this.refreshAfterChange(),
        error: () => {
          this.errorMessage.set('The action could not be updated.');
        },
      });
  }

  startEditing(action: GoalAction): void {
    this.editingActionId.set(action.id);
    this.editActionTitle.setValue(action.title);
  }

  cancelEditing(): void {
    this.editingActionId.set(null);
    this.editActionTitle.reset('');
  }

  saveEdit(action: GoalAction): void {
    const title = this.editActionTitle.value.trim();

    if (!title || this.processingActionId() !== null) {
      this.editActionTitle.markAsTouched();
      return;
    }

    this.processingActionId.set(action.id);

    this.goalsApi
      .updateAction(action.goalId, action.id, { title })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.processingActionId.set(null)),
      )
      .subscribe({
        next: () => {
          this.newActionTitle.reset('');
          this.refreshAfterChange();
        },
        error: (error) => {
          console.error('Create action failed:', error);

          this.errorMessage.set(
            error.error?.detail ?? error.error?.title ?? 'The action could not be added.',
          );
        },
      });
  }

  deleteAction(action: GoalAction): void {
    if (this.processingActionId() !== null) {
      return;
    }

    this.processingActionId.set(action.id);

    this.goalsApi
      .deleteAction(action.goalId, action.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.processingActionId.set(null)),
      )
      .subscribe({
        next: () => this.refreshAfterChange(),
        error: () => {
          this.errorMessage.set('The action could not be deleted.');
        },
      });
  }

  retry(): void {
    this.loadActions();
  }

  private refreshAfterChange(): void {
    this.loadActions();
    this.goalChanged.emit();
  }

  private loadActions(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.goalsApi
      .getActions(this.goal().id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (actions) => this.actions.set(actions),
        error: () => {
          this.errorMessage.set('The actions could not be loaded.');
        },
      });
  }
}
