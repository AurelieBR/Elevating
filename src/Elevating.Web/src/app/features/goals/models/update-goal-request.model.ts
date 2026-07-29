import { GoalPriority, GoalStatus } from './goal.enums';

export interface UpdateGoalRequest {
  title: string;
  description: string | null;
  category: string;
  priority: GoalPriority;
  status: GoalStatus;
  targetDate: string | null;
}
