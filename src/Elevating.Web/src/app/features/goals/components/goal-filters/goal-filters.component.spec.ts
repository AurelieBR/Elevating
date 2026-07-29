import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GoalFilters } from './goal-filters.component';

describe('GoalFilters', () => {
  let component: GoalFilters;
  let fixture: ComponentFixture<GoalFilters>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalFilters],
    }).compileComponents();

    fixture = TestBed.createComponent(GoalFilters);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
