import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { DeleteGoalDialog } from './delete-goal-dialog.component';

describe('DeleteGoalDialog', () => {
  let fixture: ComponentFixture<DeleteGoalDialog>;
  let component: DeleteGoalDialog;

  const goal: Goal = {
    id: 1,
    title: 'Delete test goal',
    description: 'A goal used for testing.',
    category: 'Testing',
    priority: GoalPriority.Low,
    status: GoalStatus.NotStarted,
    targetDate: null,
    createdDate: '2026-07-29T12:00:00',
    updatedDate: '2026-07-29T12:00:00',
    isOverdue: false,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteGoalDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteGoalDialog);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('goal', goal);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit confirmed when confirm is called', () => {
    const emittedValues: void[] = [];

    component.confirmed.subscribe((value) => {
      emittedValues.push(value);
    });

    component.confirm();

    expect(emittedValues).toHaveLength(1);
  });

  it('should emit cancelled when cancel is called', () => {
    const emittedValues: void[] = [];

    component.cancelled.subscribe((value) => {
      emittedValues.push(value);
    });

    component.cancel();

    expect(emittedValues).toHaveLength(1);
  });

  it('should not emit confirmed while deleting', () => {
    fixture.componentRef.setInput('deleting', true);
    fixture.detectChanges();

    let emitted = false;

    component.confirmed.subscribe(() => {
      emitted = true;
    });

    component.confirm();

    expect(emitted).toBe(false);
  });

  it('should not emit cancelled while deleting', () => {
    fixture.componentRef.setInput('deleting', true);
    fixture.detectChanges();

    let emitted = false;

    component.cancelled.subscribe(() => {
      emitted = true;
    });

    component.cancel();

    expect(emitted).toBe(false);
  });
});
