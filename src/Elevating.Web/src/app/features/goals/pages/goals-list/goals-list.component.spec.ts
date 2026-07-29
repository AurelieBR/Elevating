import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GoalsList } from './goals-list.component';

describe('GoalsList', () => {
  let component: GoalsList;
  let fixture: ComponentFixture<GoalsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalsList],
    }).compileComponents();

    fixture = TestBed.createComponent(GoalsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
