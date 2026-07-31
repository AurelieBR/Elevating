import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal,
  DestroyRef,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { PagedResult } from '../../../../core/models/paged-result.model';
import { GoalCard } from '../../components/goal-card/goal-card.component';
import { GoalFilters } from '../../components/goal-filters/goal-filters.component';
import { GoalPagination } from '../../components/goal-pagination/goal-pagination.component';
import { Goal, GoalStatus, GoalQueryParameters, GoalSummary, SortDirection } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DeleteGoalDialog } from '../../components/delete-goal-dialog/delete-goal-dialog.component';
import { GoalSummaryComponent } from '../../components/goal-summary/goal-summary.component';

@Component({
  selector: 'app-goals-list',
  imports: [
    RouterLink,
    GoalCard,
    GoalFilters,
    GoalPagination,
    DeleteGoalDialog,
    GoalSummaryComponent,
  ],
  templateUrl: './goals-list.component.html',
  styleUrl: './goals-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalsList implements OnInit {
  private readonly goalsApi = inject(GoalsApi);

  readonly result = signal<PagedResult<Goal> | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly summary = signal<GoalSummary | null>(null);
  readonly summaryLoading = signal(false);
  readonly summaryError = signal<string | null>(null);

  readonly query = signal<GoalQueryParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdDate',
    sortDirection: SortDirection.Descending,
  });

  private readonly destroyRef = inject(DestroyRef);

  readonly processingGoalId = signal<number | null>(null);
  readonly selectedGoalForDeletion = signal<Goal | null>(null);
  readonly deleting = signal(false);

  readonly notification = signal<{
    type: 'success' | 'error';
    message: string;
  } | null>(null);

  private notificationTimeout: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.loadGoals();
    this.loadSummary();
  }

  applyFilters(parameters: GoalQueryParameters): void {
    this.query.set({
      ...parameters,
      pageNumber: 1,
    });

    this.loadGoals();
  }

  changePage(pageNumber: number): void {
    this.query.update((current) => ({
      ...current,
      pageNumber,
    }));

    this.loadGoals();

    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  }

  retry(): void {
    this.loadGoals();
  }

  retrySummary(): void {
    this.loadSummary();
  }

  private loadGoals(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.goalsApi
      .getAll(this.query())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loading.set(false);
        }),
      )
      .subscribe({
        next: (result) => {
          this.result.set(result);
        },
        error: () => {
          this.errorMessage.set(
            'We could not load your goals. Make sure the API is running and try again.',
          );
        },
      });
  }

  private loadSummary(): void {
    this.summaryLoading.set(true);
    this.summaryError.set(null);

    this.goalsApi
      .getSummary()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.summaryLoading.set(false);
        }),
      )
      .subscribe({
        next: (summary) => {
          this.summary.set(summary);
        },
        error: () => {
          this.summaryError.set('The goal summary could not be loaded.');
        },
      });
  }

  markComplete(goal: Goal): void {
    if (goal.status === GoalStatus.Completed || this.processingGoalId() !== null) {
      return;
    }

    this.processingGoalId.set(goal.id);
    this.notification.set(null);

    this.goalsApi
      .complete(goal.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.processingGoalId.set(null);
        }),
      )
      .subscribe({
        next: () => {
          this.showNotification('success', `“${goal.title}” was marked as completed.`);

          this.loadGoals();
          this.loadSummary();
        },
        error: (error: HttpErrorResponse) => {
          this.showNotification(
            'error',
            this.getActionError(error, 'The goal could not be marked as completed.'),
          );
        },
      });
  }

  openDeleteDialog(goal: Goal): void {
    if (this.processingGoalId() !== null) {
      return;
    }

    this.selectedGoalForDeletion.set(goal);
  }

  closeDeleteDialog(): void {
    if (!this.deleting()) {
      this.selectedGoalForDeletion.set(null);
    }
  }

  confirmDelete(): void {
    const goal = this.selectedGoalForDeletion();

    if (goal === null || this.deleting()) {
      return;
    }

    this.deleting.set(true);
    this.processingGoalId.set(goal.id);
    this.notification.set(null);

    this.goalsApi
      .delete(goal.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.deleting.set(false);
          this.processingGoalId.set(null);
        }),
      )
      .subscribe({
        next: () => {
          this.selectedGoalForDeletion.set(null);

          this.showNotification('success', `“${goal.title}” was deleted.`);

          this.adjustPageAfterDeletion();
          this.loadGoals();
          this.loadSummary();
        },
        error: (error: HttpErrorResponse) => {
          this.showNotification(
            'error',
            this.getActionError(error, 'The goal could not be deleted.'),
          );
        },
      });
  }

  dismissNotification(): void {
    this.notification.set(null);

    if (this.notificationTimeout !== null) {
      clearTimeout(this.notificationTimeout);
      this.notificationTimeout = null;
    }
  }

  private adjustPageAfterDeletion(): void {
    const result = this.result();

    if (result !== null && result.items.length === 1 && result.pageNumber > 1) {
      this.query.update((query) => ({
        ...query,
        pageNumber: query.pageNumber - 1,
      }));
    }
  }

  private showNotification(type: 'success' | 'error', message: string): void {
    this.dismissNotification();
    this.notification.set({ type, message });

    this.notificationTimeout = setTimeout(() => {
      this.notification.set(null);
      this.notificationTimeout = null;
    }, 5000);
  }

  private getActionError(error: HttpErrorResponse, fallbackMessage: string): string {
    if (error.status === 0) {
      return 'The API could not be reached. Make sure it is running and try again.';
    }

    if (error.status === 404) {
      return 'This goal no longer exists. Refreshing the dashboard may resolve the issue.';
    }

    const problem = error.error as
      | {
          title?: string;
          detail?: string;
        }
      | undefined;

    return problem?.detail ?? problem?.title ?? fallbackMessage;
  }
}
