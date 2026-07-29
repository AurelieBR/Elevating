import { GoalPriority } from './goal.enums';

export interface CreateGoalRequest {
  title: string;
  description: string | null;
  category: string;
  priority: GoalPriority;
  targetDate: string | null;
}
