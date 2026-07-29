import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';

import { GoalPriority, GoalStatus } from '../../models';
import { GoalCard } from './goal-card.component';

describe('GoalCard', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalCard],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(GoalCard);

    fixture.componentRef.setInput('goal', {
      id: 1,
      title: 'Build Angular frontend',
      description: 'Create the goals dashboard.',
      category: 'Development',
      priority: GoalPriority.High,
      status: GoalStatus.InProgress,
      targetDate: '2026-08-15T00:00:00',
      createdDate: '2026-07-29T12:00:00',
      updatedDate: '2026-07-29T12:00:00',
    });

    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });
});
