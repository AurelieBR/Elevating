import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Goal, GoalStatus } from '../../models';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-goal-card',
  imports: [RouterLink, DatePipe],
  templateUrl: './goal-card.component.html',
  styleUrl: './goal-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalCard {
  readonly goal = input.required<Goal>();
  readonly processing = input(false);

  readonly completeGoal = output<Goal>();
  readonly deleteGoal = output<Goal>();

  readonly GoalStatus = GoalStatus;

  requestCompletion(): void {
    if (!this.processing()) {
      this.completeGoal.emit(this.goal());
    }
  }

  requestDeletion(): void {
    if (!this.processing()) {
      this.deleteGoal.emit(this.goal());
    }
  }
}
