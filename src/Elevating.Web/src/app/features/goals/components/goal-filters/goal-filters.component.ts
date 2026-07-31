import { ChangeDetectionStrategy, Component, DestroyRef, inject, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { debounceTime } from 'rxjs';

import {
  GoalPriority,
  GoalQueryParameters,
  GoalSortField,
  GoalStatus,
  SortDirection,
} from '../../models';

interface GoalFiltersForm {
  search: FormControl<string>;
  category: FormControl<string>;
  status: FormControl<GoalStatus | null>;
  priority: FormControl<GoalPriority | null>;
  isOverdue: FormControl<boolean>;
  sortBy: FormControl<GoalSortField>;
  sortDirection: FormControl<SortDirection>;
  pageSize: FormControl<number>;
}

@Component({
  selector: 'app-goal-filters',
  imports: [ReactiveFormsModule],
  templateUrl: './goal-filters.component.html',
  styleUrl: './goal-filters.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalFilters {
  private readonly destroyRef = inject(DestroyRef);

  readonly filtersChanged = output<GoalQueryParameters>();

  readonly statuses = [
    { value: GoalStatus.NotStarted, label: 'Not started' },
    { value: GoalStatus.InProgress, label: 'In progress' },
    { value: GoalStatus.Completed, label: 'Completed' },
  ];

  readonly priorities = [
    { value: GoalPriority.Low, label: 'Low' },
    { value: GoalPriority.Medium, label: 'Medium' },
    { value: GoalPriority.High, label: 'High' },
  ];

  readonly sortFields: readonly {
    value: GoalSortField;
    label: string;
  }[] = [
    { value: 'createdDate', label: 'Created date' },
    { value: 'updatedDate', label: 'Updated date' },
    { value: 'targetDate', label: 'Target date' },
    { value: 'title', label: 'Title' },
    { value: 'category', label: 'Category' },
    { value: 'priority', label: 'Priority' },
    { value: 'status', label: 'Status' },
  ];

  readonly form = new FormGroup<GoalFiltersForm>({
    search: new FormControl('', {
      nonNullable: true,
    }),
    category: new FormControl('', {
      nonNullable: true,
    }),
    status: new FormControl<GoalStatus | null>(null),
    priority: new FormControl<GoalPriority | null>(null),
    isOverdue: new FormControl(false, {
      nonNullable: true,
    }),
    sortBy: new FormControl<GoalSortField>('createdDate', {
      nonNullable: true,
    }),
    sortDirection: new FormControl<SortDirection>(SortDirection.Descending, {
      nonNullable: true,
    }),
    pageSize: new FormControl(10, {
      nonNullable: true,
    }),
  });

  constructor() {
    this.form.valueChanges
      .pipe(debounceTime(300), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.emitFilters();
      });
  }

  reset(): void {
    this.form.reset({
      search: '',
      category: '',
      status: null,
      priority: null,
      isOverdue: false,
      sortBy: 'createdDate',
      sortDirection: SortDirection.Descending,
      pageSize: 10,
    });
  }

  private emitFilters(): void {
    const value = this.form.getRawValue();

    this.filtersChanged.emit({
      pageNumber: 1,
      pageSize: value.pageSize,
      search: value.search.trim() || undefined,
      category: value.category.trim() || undefined,
      status: value.status ?? undefined,
      priority: value.priority ?? undefined,
      isOverdue: value.isOverdue || undefined,
      sortBy: value.sortBy,
      sortDirection: value.sortDirection,
    });
  }
}
