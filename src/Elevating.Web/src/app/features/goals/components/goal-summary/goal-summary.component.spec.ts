import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GoalSummaryComponent } from './goal-summary.component';

describe('GoalSummaryComponent', () => {
  let component: GoalSummaryComponent;
  let fixture: ComponentFixture<GoalSummaryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalSummaryComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(GoalSummaryComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should calculate the completion percentage', () => {
    fixture.componentRef.setInput('summary', {
      total: 10,
      notStarted: 2,
      inProgress: 3,
      completed: 5,
      overdue: 1,
    });

    fixture.detectChanges();

    expect(component.completionPercentage()).toBe(50);
  });

  it('should return zero when there are no goals', () => {
    fixture.componentRef.setInput('summary', {
      total: 0,
      notStarted: 0,
      inProgress: 0,
      completed: 0,
      overdue: 0,
    });

    fixture.detectChanges();

    expect(component.completionPercentage()).toBe(0);
  });

  it('should emit when retry is requested', () => {
    const retrySpy = vi.fn();

    component.retryRequested.subscribe(retrySpy);

    component.retry();

    expect(retrySpy).toHaveBeenCalledOnce();
  });
});
