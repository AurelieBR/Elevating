import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { GoalsApi } from '../../services/goals-api.service';
import { GoalsList } from './goals-list.component';

describe('GoalsList', () => {
  const goalsApiMock = {
    getAll: vi.fn().mockReturnValue(
      of({
        items: [],
        pageNumber: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      }),
    ),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalsList],
      providers: [
        provideRouter([]),
        {
          provide: GoalsApi,
          useValue: goalsApiMock,
        },
      ],
    }).compileComponents();

    goalsApiMock.getAll.mockClear();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(GoalsList);

    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
    expect(goalsApiMock.getAll).toHaveBeenCalledTimes(1);
  });
});
