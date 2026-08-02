import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { GoalCard } from './goal-card.component';

describe('GoalCard', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalCard],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  function createGoal(overrides: Partial<Goal> = {}): Goal {
    return {
      id: 1,
      title: 'Test goal',
      description: 'Test goal description.',
      category: 'Testing',
      priority: GoalPriority.Medium,
      status: GoalStatus.NotStarted,
      targetDate: null,
      createdDate: '2026-07-31T12:00:00',
      updatedDate: '2026-07-31T12:00:00',
      isOverdue: false,
      actionCount: 0,
      completedActionCount: 0,
      skippedActionCount: 0,
      pendingActionCount: 0,
      progressPercentage: 0,
      ...overrides,
    };
  }

  function renderGoal(goal: Goal) {
    const fixture = TestBed.createComponent(GoalCard);

    fixture.componentRef.setInput('goal', goal);
    fixture.detectChanges();

    return fixture;
  }

  it('should emit the goal when completion is requested', () => {
    const goal = createGoal({
      title: 'Complete frontend',
      status: GoalStatus.InProgress,
      priority: GoalPriority.High,
    });

    const fixture = renderGoal(goal);
    const emittedGoals: Goal[] = [];

    fixture.componentInstance.completeGoal.subscribe((value) => {
      emittedGoals.push(value);
    });

    fixture.componentInstance.requestCompletion();

    expect(emittedGoals).toEqual([goal]);
  });

  it('should emit the goal when deletion is requested', () => {
    const goal = createGoal({
      title: 'Delete test goal',
      description: null,
      priority: GoalPriority.Low,
    });

    const fixture = renderGoal(goal);
    const emittedGoals: Goal[] = [];

    fixture.componentInstance.deleteGoal.subscribe((value) => {
      emittedGoals.push(value);
    });

    fixture.componentInstance.requestDeletion();

    expect(emittedGoals).toEqual([goal]);
  });

  it('should show Continue for an unfinished goal without actions', () => {
    const goal = createGoal({
      title: 'Prepare portfolio',
      category: 'Portfolio',
      status: GoalStatus.NotStarted,
      actionCount: 0,
    });

    const fixture = renderGoal(goal);

    const primaryAction = fixture.nativeElement.querySelector(
      '[data-testid="primary-goal-action"]',
    ) as HTMLElement | null;

    expect(primaryAction).not.toBeNull();
    expect(primaryAction?.textContent?.trim()).toContain('Continue');
  });

  it('should show Continue for an unfinished goal with actions', () => {
    const goal = createGoal({
      id: 2,
      title: "Get driver's licence",
      category: 'Personal',
      priority: GoalPriority.High,
      status: GoalStatus.InProgress,
      actionCount: 5,
      completedActionCount: 2,
      pendingActionCount: 3,
      progressPercentage: 40,
    });

    const fixture = renderGoal(goal);

    const primaryAction = fixture.nativeElement.querySelector(
      '[data-testid="primary-goal-action"]',
    ) as HTMLElement | null;

    expect(primaryAction).not.toBeNull();
    expect(primaryAction?.textContent?.trim()).toContain('Continue');
  });

  it('should show View for a completed goal', () => {
    const goal = createGoal({
      id: 3,
      title: 'Complete documentation',
      category: 'Documentation',
      status: GoalStatus.Completed,
      actionCount: 3,
      completedActionCount: 3,
      pendingActionCount: 0,
      progressPercentage: 100,
    });

    const fixture = renderGoal(goal);

    const primaryAction = fixture.nativeElement.querySelector(
      '[data-testid="primary-goal-action"]',
    ) as HTMLElement | null;

    expect(primaryAction).not.toBeNull();
    expect(primaryAction?.textContent?.trim()).toContain('View');
  });

  it('should apply the completed card class to a completed goal', () => {
    const goal = createGoal({
      status: GoalStatus.Completed,
      progressPercentage: 100,
    });

    const fixture = renderGoal(goal);

    const card = fixture.nativeElement.querySelector('.goal-card') as HTMLElement | null;

    expect(card).not.toBeNull();
    expect(card?.classList.contains('goal-card-completed')).toBe(true);
  });

  it('should display the completed status', () => {
    const goal = createGoal({
      status: GoalStatus.Completed,
      progressPercentage: 100,
    });

    const fixture = renderGoal(goal);

    expect(fixture.nativeElement.textContent).toContain('Completed');
  });
});
