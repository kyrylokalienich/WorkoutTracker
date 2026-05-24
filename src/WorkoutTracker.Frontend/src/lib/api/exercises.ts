import { apiRequest } from "./client";
import type { ExerciseResponse, ExerciseFilters } from "@/types/exercise";

export function listExercises(filters: ExerciseFilters = {}): Promise<ExerciseResponse[]> {
  const params = new URLSearchParams();
  if (filters.category) params.set("category", filters.category);
  if (filters.muscleGroup) params.set("muscleGroup", filters.muscleGroup);
  if (filters.search) params.set("search", filters.search);
  const query = params.toString();
  return apiRequest<ExerciseResponse[]>(`/api/exercises${query ? `?${query}` : ""}`);
}

export function getExercise(id: number): Promise<ExerciseResponse> {
  return apiRequest<ExerciseResponse>(`/api/exercises/${id}`);
}
