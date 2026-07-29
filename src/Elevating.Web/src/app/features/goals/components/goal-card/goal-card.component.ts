import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Goal, GoalPriority, GoalStatus } from '../../models';

@Component({
  selector: 'app-goal-card',
  imports: [DatePipe, RouterLink],
  templateUrl: './goal-card.component.html',
  styleUrl: './goal-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalCard {
  readonly goal = input.required<Goal>();

  statusLabel(status: GoalStatus): string {
    switch (status) {
      case GoalStatus.NotStarted:
        return 'Not started';
      case GoalStatus.InProgress:
        return 'In progress';
      case GoalStatus.Completed:
        return 'Completed';
    }
  }

  priorityLabel(priority: GoalPriority): string {
    switch (priority) {
      case GoalPriority.Low:
        return 'Low';
      case GoalPriority.Medium:
        return 'Medium';
      case GoalPriority.High:
        return 'High';
    }
  }

  statusClasses(status: GoalStatus): string {
    switch (status) {
      case GoalStatus.NotStarted:
        return 'bg-slate-100 text-slate-700';
      case GoalStatus.InProgress:
        return 'bg-blue-100 text-blue-700';
      case GoalStatus.Completed:
        return 'bg-emerald-100 text-emerald-700';
    }
  }

  priorityClasses(priority: GoalPriority): string {
    switch (priority) {
      case GoalPriority.Low:
        return 'bg-green-50 text-green-700 ring-green-600/20';
      case GoalPriority.Medium:
        return 'bg-amber-50 text-amber-700 ring-amber-600/20';
      case GoalPriority.High:
        return 'bg-rose-50 text-rose-700 ring-rose-600/20';
    }
  }
}
