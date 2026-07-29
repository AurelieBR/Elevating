import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { PagedResult } from '../../../core/models/paged-result.model';
import { CreateGoalRequest, Goal, GoalQueryParameters, UpdateGoalRequest } from '../models';

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

  getById(id: number): Observable<Goal> {
    return this.http.get<Goal>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateGoalRequest): Observable<Goal> {
    return this.http.post<Goal>(this.baseUrl, request);
  }

  update(id: number, request: UpdateGoalRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  complete(id: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/complete`, null);
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
