import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { GoalCard } from './goal-card.component';

describe('GoalCard', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalCard],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should emit the goal when completion is requested', () => {
    const fixture = TestBed.createComponent(GoalCard);

    const goal: Goal = {
      id: 1,
      title: 'Complete frontend',
      description: 'Finish the dashboard actions.',
      category: 'Development',
      priority: GoalPriority.High,
      status: GoalStatus.InProgress,
      targetDate: '2026-08-10T00:00:00',
      createdDate: '2026-07-29T12:00:00',
      updatedDate: '2026-07-29T12:00:00',
    };

    fixture.componentRef.setInput('goal', goal);
    fixture.detectChanges();

    const emittedGoals: Goal[] = [];

    fixture.componentInstance.completeGoal.subscribe((value) => {
      emittedGoals.push(value);
    });

    fixture.componentInstance.requestCompletion();

    expect(emittedGoals).toEqual([goal]);
  });

  it('should emit the goal when deletion is requested', () => {
    const fixture = TestBed.createComponent(GoalCard);

    const goal: Goal = {
      id: 1,
      title: 'Delete test goal',
      description: null,
      category: 'Testing',
      priority: GoalPriority.Low,
      status: GoalStatus.NotStarted,
      targetDate: null,
      createdDate: '2026-07-29T12:00:00',
      updatedDate: '2026-07-29T12:00:00',
    };

    fixture.componentRef.setInput('goal', goal);
    fixture.detectChanges();

    const emittedGoals: Goal[] = [];

    fixture.componentInstance.deleteGoal.subscribe((value) => {
      emittedGoals.push(value);
    });

    fixture.componentInstance.requestDeletion();

    expect(emittedGoals).toEqual([goal]);
  });
});
