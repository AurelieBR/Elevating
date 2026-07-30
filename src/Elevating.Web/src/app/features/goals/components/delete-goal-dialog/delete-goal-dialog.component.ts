import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { Goal } from '../../models';

@Component({
  selector: 'app-delete-goal-dialog',
  templateUrl: './delete-goal-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeleteGoalDialog {
  readonly goal = input.required<Goal>();
  readonly deleting = input(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  confirm(): void {
    if (!this.deleting()) {
      this.confirmed.emit();
    }
  }

  cancel(): void {
    if (!this.deleting()) {
      this.cancelled.emit();
    }
  }
}
