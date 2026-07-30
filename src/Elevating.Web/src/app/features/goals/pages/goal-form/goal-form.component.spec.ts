import { convertToParamMap } from '@angular/router';
import { ActivatedRoute, Router } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { GoalPriority, GoalStatus } from '../../models';
import { GoalsApi } from '../../services/goals-api.service';
import { GoalForm } from './goal-form.component';

describe('GoalForm', () => {
  const goalsApiMock = {
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
  };

  const routerMock = {
    navigate: vi.fn().mockResolvedValue(true),
  };

  function configureTest(id: string | null = null): void {
    TestBed.configureTestingModule({
      imports: [GoalForm],
      providers: [
        {
          provide: GoalsApi,
          useValue: goalsApiMock,
        },
        {
          provide: Router,
          useValue: routerMock,
        },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap(id === null ? {} : { id }),
            },
          },
        },
      ],
    });
  }

  beforeEach(() => {
    goalsApiMock.getById.mockReset();
    goalsApiMock.create.mockReset();
    goalsApiMock.update.mockReset();
    routerMock.navigate.mockClear();
  });

  it('should create the component in create mode', () => {
    configureTest();

    const fixture = TestBed.createComponent(GoalForm);

    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.componentInstance.isEditMode()).toBe(false);
    expect(goalsApiMock.getById).not.toHaveBeenCalled();
  });

  it('should not submit an invalid form', () => {
    configureTest();

    const fixture = TestBed.createComponent(GoalForm);
    const component = fixture.componentInstance;

    fixture.detectChanges();

    component.submit();

    expect(component.form.invalid).toBe(true);
    expect(component.form.controls.title.touched).toBe(true);
    expect(goalsApiMock.create).not.toHaveBeenCalled();
  });

  it('should create a valid goal and navigate home', () => {
    goalsApiMock.create.mockReturnValue(
      of({
        id: 1,
        title: 'Build Angular form',
        category: 'Development',
        description: null,
        priority: GoalPriority.High,
        status: GoalStatus.NotStarted,
        targetDate: null,
        createdDate: '2026-07-29T12:00:00',
        updatedDate: '2026-07-29T12:00:00',
      }),
    );

    configureTest();

    const fixture = TestBed.createComponent(GoalForm);
    const component = fixture.componentInstance;

    fixture.detectChanges();

    component.form.patchValue({
      title: ' Build Angular form ',
      category: ' Development ',
      description: ' ',
      priority: GoalPriority.High,
      targetDate: '',
    });

    component.submit();

    expect(goalsApiMock.create).toHaveBeenCalledWith({
      title: 'Build Angular form',
      category: 'Development',
      description: null,
      priority: GoalPriority.High,
      targetDate: null,
    });

    expect(routerMock.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should load and update an existing goal', () => {
    goalsApiMock.getById.mockReturnValue(
      of({
        id: 7,
        title: 'Existing goal',
        category: 'Development',
        description: 'Existing description',
        priority: GoalPriority.Medium,
        status: GoalStatus.InProgress,
        targetDate: '2026-08-20T00:00:00',
        createdDate: '2026-07-20T12:00:00',
        updatedDate: '2026-07-28T12:00:00',
      }),
    );

    goalsApiMock.update.mockReturnValue(of(void 0));

    configureTest('7');

    const fixture = TestBed.createComponent(GoalForm);
    const component = fixture.componentInstance;

    fixture.detectChanges();

    expect(component.isEditMode()).toBe(true);
    expect(goalsApiMock.getById).toHaveBeenCalledWith(7);
    expect(component.form.controls.title.value).toBe('Existing goal');
    expect(component.form.controls.targetDate.value).toBe('2026-08-20');

    component.form.patchValue({
      title: 'Updated goal',
      status: GoalStatus.Completed,
    });

    component.submit();

    expect(goalsApiMock.update).toHaveBeenCalledWith(
      7,
      expect.objectContaining({
        title: 'Updated goal',
        status: GoalStatus.Completed,
      }),
    );

    expect(routerMock.navigate).toHaveBeenCalledWith(['/']);
  });
});
