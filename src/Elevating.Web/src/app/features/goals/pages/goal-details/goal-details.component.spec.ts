import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { GoalDetails } from './goal-details.component';

describe('GoalDetails', () => {
  let fixture: ComponentFixture<GoalDetails>;
  let component: GoalDetails;
  let httpTestingController: HttpTestingController;

  const goal: Goal = {
    id: 1,
    title: 'Complete goal details',
    description: 'Finish the details page.',
    category: 'Development',
    priority: GoalPriority.High,
    status: GoalStatus.InProgress,
    targetDate: '2026-08-10T00:00:00',
    createdDate: '2026-07-29T12:00:00',
    updatedDate: '2026-07-30T12:00:00',
    isOverdue: false,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalDetails],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({
                id: '1',
              }),
            },
          },
        },
      ],
    }).compileComponents();

    httpTestingController = TestBed.inject(HttpTestingController);

    fixture = TestBed.createComponent(GoalDetails);
    component = fixture.componentInstance;

    fixture.detectChanges();

    const request = httpTestingController.expectOne(
      (req) => req.method === 'GET' && req.url.toLowerCase().endsWith('/goals/1'),
    );

    request.flush(goal);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load the goal', () => {
    expect(component.goal()).toEqual(goal);
    expect(component.loading()).toBe(false);
  });
});
