import { GoalPriority, GoalStatus } from './goal.enums';

export interface Goal {
  id: number;
  title: string;
  description: string | null;
  category: string;
  priority: GoalPriority;
  status: GoalStatus;
  targetDate: string | null;
  createdDate: string;
  updatedDate: string;
  isOverdue: boolean;
  actionCount: number;
  completedActionCount: number;
  skippedActionCount: number;
  pendingActionCount: number;
  progressPercentage: number;
}
