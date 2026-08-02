import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';
import { GoalActions } from './goal-actions.component';

describe('GoalActions', () => {
  let component: GoalActions;
  let fixture: ComponentFixture<GoalActions>;

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
    actionCount: 0,
    completedActionCount: 0,
    skippedActionCount: 0,
    pendingActionCount: 0,
    progressPercentage: 0,
  };

  const goalsApiMock = {
    getActions: vi.fn(),
    createAction: vi.fn(),
    updateAction: vi.fn(),
    completeAction: vi.fn(),
    reopenAction: vi.fn(),
    deleteAction: vi.fn(),
  };

  beforeEach(async () => {
    goalsApiMock.getActions.mockReset();
    goalsApiMock.getActions.mockReturnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [GoalActions],
      providers: [
        {
          provide: GoalsApi,
          useValue: goalsApiMock,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GoalActions);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('goal', goal);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load the goal actions', () => {
    expect(goalsApiMock.getActions).toHaveBeenCalledWith(goal.id);
    expect(component.actions()).toEqual([]);
    expect(component.loading()).toBe(false);
  });
});
