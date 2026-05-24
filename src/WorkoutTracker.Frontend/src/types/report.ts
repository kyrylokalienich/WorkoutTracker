export interface WorkoutHistoryItem {
  id: number;
  title: string;
  completedAtUtc: string;
  exerciseCount: number;
  totalVolumeKg: number;
}

export interface WorkoutHistoryResponse {
  items: WorkoutHistoryItem[];
}

export interface ProgressReportResponse {
  completedWorkoutCount: number;
  totalVolumeKg: number;
  averageVolumeKgPerWorkout: number;
  scheduledCompletedCount: number;
  scheduledSkippedCount: number;
  completionRate: number;
}

export interface MuscleGroupVolumeItem {
  muscleGroup: string;
  totalVolumeKg: number;
  sessionExerciseLineCount: number;
}

export interface MuscleGroupVolumeResponse {
  items: MuscleGroupVolumeItem[];
}

export interface DateRangeFilter {
  from?: string;
  to?: string;
}
