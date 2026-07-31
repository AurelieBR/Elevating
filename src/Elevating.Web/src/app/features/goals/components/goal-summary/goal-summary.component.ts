import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { GoalSummary } from '../../models';

@Component({
  selector: 'app-goal-summary',
  templateUrl: './goal-summary.component.html',
  styleUrl: './goal-summary.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalSummaryComponent {
  readonly summary = input<GoalSummary | null>(null);
  readonly loading = input(false);
  readonly errorMessage = input<string | null>(null);

  readonly retryRequested = output<void>();

  readonly completionPercentage = computed(() => {
    const summary = this.summary();

    if (summary === null || summary.total === 0) {
      return 0;
    }

    return Math.round((summary.completed / summary.total) * 100);
  });

  readonly progressChartStyle = computed(() => {
    const percentage = this.completionPercentage();

    return {
      background: `conic-gradient(
        var(--color-sage) 0% ${percentage}%,
        var(--color-surface-muted) ${percentage}% 100%
      )`,
    };
  });

  retry(): void {
    this.retryRequested.emit();
  }
}
