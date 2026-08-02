import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { CompleteGoalDialog } from './complete-goal-dialog.component';

describe('CompleteGoalDialog', () => {
  let component: CompleteGoalDialog;
  let fixture: ComponentFixture<CompleteGoalDialog>;

  const goal: Goal = {
    id: 1,
    title: "Get driver's licence",
    description: 'Complete every step required for the licence.',
    category: 'Personal',
    priority: GoalPriority.High,
    status: GoalStatus.InProgress,
    targetDate: '2026-09-30T00:00:00',
    createdDate: '2026-07-29T12:00:00',
    updatedDate: '2026-07-30T12:00:00',
    isOverdue: false,
    actionCount: 5,
    completedActionCount: 3,
    skippedActionCount: 0,
    pendingActionCount: 2,
    progressPercentage: 60,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompleteGoalDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(CompleteGoalDialog);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('goal', goal);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit when completing all remaining actions', () => {
    const emitted = vi.fn();

    component.completeAll.subscribe(emitted);
    component.completeAll.emit();

    expect(emitted).toHaveBeenCalledOnce();
  });

  it('should emit when skipping remaining actions', () => {
    const emitted = vi.fn();

    component.skipRemaining.subscribe(emitted);
    component.skipRemaining.emit();

    expect(emitted).toHaveBeenCalledOnce();
  });

  it('should emit when cancelled', () => {
    const emitted = vi.fn();

    component.cancelled.subscribe(emitted);
    component.cancelled.emit();

    expect(emitted).toHaveBeenCalledOnce();
  });
});
