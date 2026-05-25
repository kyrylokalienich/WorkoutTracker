import { apiRequest } from "./client";
import type {
  WorkoutSessionSummaryResponse,
  WorkoutSessionDetailResponse,
  PaginatedResponse,
  ScheduleSessionRequest,
  UpdateSessionRequest,
  CompleteSessionRequest,
  AddSessionExerciseRequest,
  SessionFilters,
} from "@/types/session";

export function scheduleSSession(req: ScheduleSessionRequest): Promise<WorkoutSessionDetailResponse> {
  return apiRequest<WorkoutSessionDetailResponse>("/api/workout-sessions/schedule", {
    method: "POST",
    body: JSON.stringify(req),
  });
}

export function listSessions(
  filters: SessionFilters = {}
): Promise<PaginatedResponse<WorkoutSessionSummaryResponse>> {
  const params = new URLSearchParams();
  if (filters.status) params.set("status", filters.status);
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  const query = params.toString();
  return apiRequest<PaginatedResponse<WorkoutSessionSummaryResponse>>(
    `/api/workout-sessions${query ? `?${query}` : ""}`
  );
}

export function getSession(id: number): Promise<WorkoutSessionDetailResponse> {
  return apiRequest<WorkoutSessionDetailResponse>(`/api/workout-sessions/${id}`);
}

export function updateSession(
  id: number,
  req: UpdateSessionRequest
): Promise<WorkoutSessionDetailResponse> {
  return apiRequest<WorkoutSessionDetailResponse>(`/api/workout-sessions/${id}`, {
    method: "PUT",
    body: JSON.stringify(req),
  });
}

export function completeSession(
  id: number,
  req: CompleteSessionRequest
): Promise<WorkoutSessionDetailResponse> {
  return apiRequest<WorkoutSessionDetailResponse>(`/api/workout-sessions/${id}/complete`, {
    method: "POST",
    body: JSON.stringify(req),
  });
}

export function addSessionExercise(
  sessionId: number,
  req: AddSessionExerciseRequest
): Promise<WorkoutSessionDetailResponse> {
  return apiRequest<WorkoutSessionDetailResponse>(
    `/api/workout-sessions/${sessionId}/exercises`,
    { method: "POST", body: JSON.stringify(req) }
  );
}

export function deleteSession(id: number): Promise<void> {
  return apiRequest<void>(`/api/workout-sessions/${id}`, { method: "DELETE" });
}
