import { apiRequest } from "./client";
import type {
  WorkoutHistoryResponse,
  ProgressReportResponse,
  MuscleGroupVolumeResponse,
  DateRangeFilter,
} from "@/types/report";

function buildQuery(filters: DateRangeFilter): string {
  const params = new URLSearchParams();
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  const query = params.toString();
  return query ? `?${query}` : "";
}

export function getWorkoutHistory(filters: DateRangeFilter = {}): Promise<WorkoutHistoryResponse> {
  return apiRequest<WorkoutHistoryResponse>(`/api/reports/workout-history${buildQuery(filters)}`);
}

export function getProgressReport(filters: DateRangeFilter = {}): Promise<ProgressReportResponse> {
  return apiRequest<ProgressReportResponse>(`/api/reports/progress${buildQuery(filters)}`);
}

export function getMuscleGroupVolume(filters: DateRangeFilter = {}): Promise<MuscleGroupVolumeResponse> {
  return apiRequest<MuscleGroupVolumeResponse>(`/api/reports/muscle-groups${buildQuery(filters)}`);
}
