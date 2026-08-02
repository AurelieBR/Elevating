import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { Goal } from '../../models';

@Component({
  selector: 'app-complete-goal-dialog',
  templateUrl: './complete-goal-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompleteGoalDialog {
  readonly goal = input.required<Goal>();
  readonly processing = input(false);

  readonly completeAll = output<void>();
  readonly skipRemaining = output<void>();
  readonly cancelled = output<void>();
}
