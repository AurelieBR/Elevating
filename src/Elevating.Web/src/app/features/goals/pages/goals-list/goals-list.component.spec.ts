import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { GoalsList } from './goals-list.component';

describe('GoalsList', () => {
  let fixture: ComponentFixture<GoalsList>;
  let component: GoalsList;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalsList],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(GoalsList);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
