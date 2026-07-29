import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-goal-card',
  imports: [],
  templateUrl: './goal-card.component.html',
  styleUrl: './goal-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalCard {}
