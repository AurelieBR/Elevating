import { GoalActionStatus } from './goal.enums';

export interface GoalAction {
  id: number;
  goalId: number;
  title: string;
  status: GoalActionStatus;
  position: number;
  createdDate: string;
  updatedDate: string;
}

export interface CreateGoalActionRequest {
  title: string;
}

export interface UpdateGoalActionRequest {
  title: string;
}
