import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { DeleteGoalDialog } from '../../components/delete-goal-dialog/delete-goal-dialog.component';
import { Goal, GoalPriority, GoalStatus } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';
import { RemainingActionsResolution } from '../../models';

import { GoalActions } from '../../components/goal-actions/goal-actions.component';

import { CompleteGoalDialog } from '../../components/complete-goal-dialog/complete-goal-dialog.component';

type ArrivalNotice = 'created' | 'updated';

@Component({
  selector: 'app-goal-details',
  imports: [DatePipe, RouterLink, DeleteGoalDialog, GoalActions, CompleteGoalDialog],
  templateUrl: './goal-details.component.html',
  styleUrl: './goal-details.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalDetails implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly goalsApi = inject(GoalsApi);
  private readonly destroyRef = inject(DestroyRef);

  readonly goal = signal<Goal | null>(null);
  readonly loading = signal(true);
  readonly processing = signal(false);
  readonly deleteDialogOpen = signal(false);

  readonly errorMessage = signal<string | null>(null);
  readonly notification = signal<{
    type: 'success' | 'error';
    message: string;
  } | null>(null);

  readonly arrivalNotice = signal<ArrivalNotice | null>(null);

  readonly GoalStatus = GoalStatus;
  readonly GoalPriority = GoalPriority;

  readonly completeDialogOpen = signal(false);

  private goalId: number | null = null;

  dismissArrivalNotice(): void {
    this.arrivalNotice.set(null);
  }

  private initializeArrivalNotice(): void {
    const notice = this.route.snapshot.queryParamMap.get('notice');

    if (notice === 'created' || notice === 'updated') {
      this.arrivalNotice.set(notice);
    }
  }

  scrollToActions(): void {
    const section = document.getElementById('goal-actions');

    if (!section) {
      return;
    }

    section.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    });

    window.setTimeout(() => {
      const actionInput = document.getElementById('new-goal-action') as HTMLInputElement | null;

      actionInput?.focus({
        preventScroll: true,
      });
    }, 500);
  }

  ngOnInit(): void {
    this.initializeArrivalNotice();

    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.loading.set(false);
      this.errorMessage.set('The goal ID is invalid.');
      return;
    }

    this.goalId = id;
    this.loadGoal();
  }

  markComplete(): void {
    const goal = this.goal();

    if (goal === null || goal.status === GoalStatus.Completed || this.processing()) {
      return;
    }

    if (goal.pendingActionCount > 0) {
      this.completeDialogOpen.set(true);
      return;
    }

    this.completeGoal(null);
  }

  completeRemainingActions(): void {
    this.completeGoal(RemainingActionsResolution.Complete);
  }

  skipRemainingActions(): void {
    this.completeGoal(RemainingActionsResolution.Skip);
  }

  closeCompleteDialog(): void {
    if (!this.processing()) {
      this.completeDialogOpen.set(false);
    }
  }

  actionsChanged(): void {
    this.arrivalNotice.set(null);
    this.loadGoal();
  }

  private completeGoal(resolution: RemainingActionsResolution | null): void {
    const goal = this.goal();

    if (goal === null || this.processing()) {
      return;
    }

    this.processing.set(true);
    this.notification.set(null);

    this.goalsApi
      .complete(goal.id, resolution)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.processing.set(false);
        }),
      )
      .subscribe({
        next: () => {
          this.completeDialogOpen.set(false);

          this.notification.set({
            type: 'success',
            message: 'Beautiful work — this goal is complete.',
          });

          this.loadGoal();
        },
        error: () => {
          this.notification.set({
            type: 'error',
            message: 'The goal could not be marked as completed.',
          });
        },
      });
  }

  openDeleteDialog(): void {
    if (!this.processing()) {
      this.deleteDialogOpen.set(true);
    }
  }

  closeDeleteDialog(): void {
    if (!this.processing()) {
      this.deleteDialogOpen.set(false);
    }
  }

  confirmDelete(): void {
    const goal = this.goal();

    if (goal === null || this.processing()) {
      return;
    }

    this.processing.set(true);
    this.notification.set(null);

    this.goalsApi
      .delete(goal.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.processing.set(false);
        }),
      )
      .subscribe({
        next: () => {
          void this.router.navigate(['/goals']);
        },
        error: () => {
          this.notification.set({
            type: 'error',
            message: 'The goal could not be deleted.',
          });
        },
      });
  }

  dismissNotification(): void {
    this.notification.set(null);
  }

  retry(): void {
    this.loadGoal();
  }

  private loadGoal(): void {
    if (this.goalId === null) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    this.goalsApi
      .getById(this.goalId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe({
        next: (goal) => {
          this.goal.set(goal);
        },
        error: (error) => {
          this.goal.set(null);

          this.errorMessage.set(
            error.status === 404
              ? 'This goal could not be found.'
              : 'We could not load this goal. Make sure the API is running and try again.',
          );
        },
      });
  }
}
