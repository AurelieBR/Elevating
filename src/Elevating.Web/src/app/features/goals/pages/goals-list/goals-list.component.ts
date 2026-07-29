import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { PagedResult } from '../../../../core/models/paged-result.model';
import { GoalCard } from '../../components/goal-card/goal-card.component';
import { GoalFilters } from '../../components/goal-filters/goal-filters.component';
import { GoalPagination } from '../../components/goal-pagination/goal-pagination.component';
import { Goal, GoalQueryParameters, SortDirection } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';

@Component({
  selector: 'app-goals-list',
  imports: [RouterLink, GoalCard, GoalFilters, GoalPagination],
  templateUrl: './goals-list.component.html',
  styleUrl: './goals-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalsList implements OnInit {
  private readonly goalsApi = inject(GoalsApi);

  readonly result = signal<PagedResult<Goal> | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly query = signal<GoalQueryParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'createdDate',
    sortDirection: SortDirection.Descending,
  });

  ngOnInit(): void {
    this.loadGoals();
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

  private loadGoals(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.goalsApi
      .getAll(this.query())
      .pipe(
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
}
