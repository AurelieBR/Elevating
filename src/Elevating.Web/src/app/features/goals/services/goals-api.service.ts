import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { PagedResult } from '../../../core/models/paged-result.model';
import {
  CreateGoalActionRequest,
  CreateGoalRequest,
  Goal,
  GoalAction,
  GoalQueryParameters,
  GoalSummary,
  RemainingActionsResolution,
  UpdateGoalActionRequest,
  UpdateGoalRequest,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class GoalsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/goals';

  getAll(parameters: GoalQueryParameters): Observable<PagedResult<Goal>> {
    return this.http.get<PagedResult<Goal>>(this.baseUrl, {
      params: this.buildQueryParams(parameters),
    });
  }

  getSummary(): Observable<GoalSummary> {
    return this.http.get<GoalSummary>(`${this.baseUrl}/summary`);
  }

  getById(id: number): Observable<Goal> {
    return this.http.get<Goal>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateGoalRequest): Observable<Goal> {
    return this.http.post<Goal>(this.baseUrl, request);
  }

  update(id: number, request: UpdateGoalRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  complete(
    id: number,
    remainingActionsResolution: RemainingActionsResolution | null = null,
  ): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/complete`, { remainingActionsResolution });
  }

  getActions(goalId: number): Observable<GoalAction[]> {
    return this.http.get<GoalAction[]>(`${this.baseUrl}/${goalId}/actions`);
  }

  createAction(goalId: number, request: CreateGoalActionRequest): Observable<GoalAction> {
    return this.http.post<GoalAction>(`${this.baseUrl}/${goalId}/actions`, request);
  }

  updateAction(
    goalId: number,
    actionId: number,
    request: UpdateGoalActionRequest,
  ): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${goalId}/actions/${actionId}`, request);
  }

  completeAction(goalId: number, actionId: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${goalId}/actions/${actionId}/complete`, null);
  }

  reopenAction(goalId: number, actionId: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${goalId}/actions/${actionId}/reopen`, null);
  }

  deleteAction(goalId: number, actionId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${goalId}/actions/${actionId}`);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private buildQueryParams(parameters: GoalQueryParameters): HttpParams {
    let params = new HttpParams()
      .set('pageNumber', parameters.pageNumber.toString())
      .set('pageSize', parameters.pageSize.toString());

    if (parameters.status !== undefined) {
      params = params.set('status', parameters.status.toString());
    }

    if (parameters.priority !== undefined) {
      params = params.set('priority', parameters.priority.toString());
    }

    if (parameters.isOverdue !== undefined) {
      params = params.set('isOverdue', parameters.isOverdue.toString());
    }

    if (parameters.category?.trim()) {
      params = params.set('category', parameters.category.trim());
    }

    if (parameters.search?.trim()) {
      params = params.set('search', parameters.search.trim());
    }

    if (parameters.sortBy) {
      params = params.set('sortBy', parameters.sortBy);
    }

    if (parameters.sortDirection !== undefined) {
      params = params.set('sortDirection', parameters.sortDirection.toString());
    }

    return params;
  }
}
