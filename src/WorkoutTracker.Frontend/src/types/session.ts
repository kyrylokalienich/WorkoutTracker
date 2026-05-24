export enum WorkoutStatus {
  Planned = "Planned",
  InProgress = "InProgress",
  Completed = "Completed",
  Skipped = "Skipped",
}

export interface WorkoutSessionExerciseResponse {
  id: number;
  exerciseId: number;
  exerciseName: string;
  plannedSets: number;
  plannedReps: number;
  plannedWeightKg: number | null;
  actualSets: number | null;
  actualReps: number | null;
  actualWeightKg: number | null;
  notes: string | null;
}

export interface WorkoutSessionSummaryResponse {
  id: number;
  workoutPlanId: number | null;
  title: string;
  scheduledAtUtc: string;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
  status: WorkoutStatus;
  exerciseCount: number;
}

export interface WorkoutSessionDetailResponse {
  id: number;
  workoutPlanId: number | null;
  title: string;
  scheduledAtUtc: string;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
  status: WorkoutStatus;
  comments: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  exercises: WorkoutSessionExerciseResponse[];
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ScheduleSessionRequest {
  workoutPlanId?: number;
  title: string;
  scheduledAtUtc: string;
}

export interface UpdateSessionRequest {
  title: string;
  scheduledAtUtc: string;
  comments?: string;
  status?: WorkoutStatus;
}

export interface CompleteSessionExercise {
  sessionExerciseId: number;
  actualSets: number;
  actualReps: number;
  actualWeightKg?: number;
  notes?: string;
}

export interface CompleteSessionRequest {
  exercises: CompleteSessionExercise[];
}

export interface SessionFilters {
  status?: WorkoutStatus;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}
