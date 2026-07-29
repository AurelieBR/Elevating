import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-goal-pagination',
  imports: [],
  templateUrl: './goal-pagination.component.html',
  styleUrl: './goal-pagination.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalPagination {
  readonly pageNumber = input(1);
  readonly totalPages = input(1);
  readonly hasPreviousPage = input(false);
  readonly hasNextPage = input(false);

  readonly pageChanged = output<number>();

  readonly visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.pageNumber();

    if (total <= 5) {
      return Array.from({ length: total }, (_, index) => index + 1);
    }

    let start = Math.max(1, current - 2);
    const end = Math.min(total, start + 4);

    if (end - start < 4) {
      start = Math.max(1, end - 4);
    }

    return Array.from({ length: end - start + 1 }, (_, index) => start + index);
  });

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.pageNumber()) {
      return;
    }

    this.pageChanged.emit(page);
  }
}
