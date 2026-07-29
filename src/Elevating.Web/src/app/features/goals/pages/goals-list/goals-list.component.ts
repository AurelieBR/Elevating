import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-goals-list',
  imports: [],
  templateUrl: './goals-list.component.html',
  styleUrl: './goals-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GoalsList {}
