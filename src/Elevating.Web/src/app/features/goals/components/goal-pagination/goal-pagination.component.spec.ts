import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GoalPagination } from './goal-pagination.component';

describe('GoalPagination', () => {
  let component: GoalPagination;
  let fixture: ComponentFixture<GoalPagination>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalPagination],
    }).compileComponents();

    fixture = TestBed.createComponent(GoalPagination);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
