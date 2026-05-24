export interface WorkoutPlanExerciseResponse {
  id: number;
  exerciseId: number;
  exerciseName: string;
  targetSets: number;
  targetReps: number;
  targetWeightKg: number | null;
  orderIndex: number;
}

export interface WorkoutPlanSummaryResponse {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  exerciseCount: number;
}

export interface WorkoutPlanDetailResponse {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  exercises: WorkoutPlanExerciseResponse[];
}

export interface CreatePlanRequest {
  name: string;
  description?: string;
  isActive: boolean;
}

export interface UpdatePlanRequest {
  name: string;
  description?: string;
  isActive: boolean;
}

export interface AddPlanExerciseRequest {
  exerciseId: number;
  targetSets: number;
  targetReps: number;
  targetWeightKg?: number;
  orderIndex: number;
}

export interface UpdatePlanExerciseRequest {
  exerciseId: number;
  targetSets: number;
  targetReps: number;
  targetWeightKg?: number;
  orderIndex: number;
}
