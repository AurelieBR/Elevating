import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-goal-form',
  imports: [],
  templateUrl: './goal-form.component.html',
  styleUrl: './goal-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalForm {}
