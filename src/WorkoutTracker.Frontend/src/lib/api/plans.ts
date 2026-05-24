import { apiRequest } from "./client";
import type {
  WorkoutPlanSummaryResponse,
  WorkoutPlanDetailResponse,
  WorkoutPlanExerciseResponse,
  CreatePlanRequest,
  UpdatePlanRequest,
  AddPlanExerciseRequest,
  UpdatePlanExerciseRequest,
} from "@/types/plan";

export function listPlans(): Promise<WorkoutPlanSummaryResponse[]> {
  return apiRequest<WorkoutPlanSummaryResponse[]>("/api/workout-plans");
}

export function getPlan(id: number): Promise<WorkoutPlanDetailResponse> {
  return apiRequest<WorkoutPlanDetailResponse>(`/api/workout-plans/${id}`);
}

export function createPlan(req: CreatePlanRequest): Promise<WorkoutPlanDetailResponse> {
  return apiRequest<WorkoutPlanDetailResponse>("/api/workout-plans", {
    method: "POST",
    body: JSON.stringify(req),
  });
}

export function updatePlan(id: number, req: UpdatePlanRequest): Promise<WorkoutPlanDetailResponse> {
  return apiRequest<WorkoutPlanDetailResponse>(`/api/workout-plans/${id}`, {
    method: "PUT",
    body: JSON.stringify(req),
  });
}

export function deletePlan(id: number): Promise<void> {
  return apiRequest<void>(`/api/workout-plans/${id}`, { method: "DELETE" });
}

export function addPlanExercise(
  planId: number,
  req: AddPlanExerciseRequest
): Promise<WorkoutPlanExerciseResponse> {
  return apiRequest<WorkoutPlanExerciseResponse>(
    `/api/workout-plans/${planId}/exercises`,
    { method: "POST", body: JSON.stringify(req) }
  );
}

export function updatePlanExercise(
  planId: number,
  planExerciseId: number,
  req: UpdatePlanExerciseRequest
): Promise<WorkoutPlanExerciseResponse> {
  return apiRequest<WorkoutPlanExerciseResponse>(
    `/api/workout-plans/${planId}/exercises/${planExerciseId}`,
    { method: "PUT", body: JSON.stringify(req) }
  );
}

export function deletePlanExercise(planId: number, planExerciseId: number): Promise<void> {
  return apiRequest<void>(
    `/api/workout-plans/${planId}/exercises/${planExerciseId}`,
    { method: "DELETE" }
  );
}
