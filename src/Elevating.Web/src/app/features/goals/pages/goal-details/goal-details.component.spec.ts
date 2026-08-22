import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';

import { Goal, GoalPriority, GoalStatus } from '../../models';
import { GoalDetails } from './goal-details.component';

describe('GoalDetails', () => {
  let fixture: ComponentFixture<GoalDetails>;
  let component: GoalDetails;
  let httpTestingController: HttpTestingController;
  let router: Router;

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
    actionCount: 0,
    completedActionCount: 0,
    skippedActionCount: 0,
    pendingActionCount: 0,
    progressPercentage: 0,
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
              queryParamMap: convertToParamMap({
                notice: 'created',
              }),
            },
          },
        },
      ],
    }).compileComponents();

    httpTestingController = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);

    fixture = TestBed.createComponent(GoalDetails);
    component = fixture.componentInstance;

    fixture.detectChanges();

    const goalRequest = httpTestingController.expectOne(
      (request) => request.method === 'GET' && request.url.toLowerCase().endsWith('/goals/1'),
    );

    goalRequest.flush(goal);
    fixture.detectChanges();

    const actionsRequest = httpTestingController.expectOne(
      (request) =>
        request.method === 'GET' && request.url.toLowerCase().endsWith('/goals/1/actions'),
    );

    actionsRequest.flush([]);
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

  it('should show the post-creation actions prompt', () => {
    expect(component.arrivalNotice()).toBe('created');

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Your goal is ready');
    expect(text).toContain('Add first action');
  });

  it('should dismiss the arrival notice', () => {
    component.dismissArrivalNotice();

    expect(component.arrivalNotice()).toBeNull();
  });

  it('should return to /goals after deletion', () => {
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    component.confirmDelete();

    const request = httpTestingController.expectOne(
      (candidate) =>
        candidate.method === 'DELETE' && candidate.url.toLowerCase().endsWith('/goals/1'),
    );
    request.flush(null);

    expect(router.navigate).toHaveBeenCalledWith(['/goals']);
  });
});
