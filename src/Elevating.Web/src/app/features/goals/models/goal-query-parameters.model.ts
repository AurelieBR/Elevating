import { GoalPriority, GoalStatus, SortDirection } from './goal.enums';

export type GoalSortField =
  'title' | 'category' | 'priority' | 'status' | 'targetDate' | 'createdDate' | 'updatedDate';

export interface GoalQueryParameters {
  pageNumber: number;
  pageSize: number;
  status?: GoalStatus;
  priority?: GoalPriority;
  category?: string;
  search?: string;
  sortBy?: GoalSortField;
  sortDirection?: SortDirection;
}
