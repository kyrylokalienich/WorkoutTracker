"use client";

import { useCallback, useEffect, useState } from "react";
import {
  scheduleSSession,
  listSessions,
  getSession,
  updateSession,
  completeSession,
  deleteSession,
} from "@/lib/api/sessions";
import type {
  WorkoutSessionSummaryResponse,
  WorkoutSessionDetailResponse,
  ScheduleSessionRequest,
  UpdateSessionRequest,
  CompleteSessionRequest,
  SessionFilters,
  WorkoutStatus,
} from "@/types/session";

const PAGE_SIZE = 10;

export function useWorkoutSessions() {
  const [sessions, setSessions] = useState<WorkoutSessionSummaryResponse[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<WorkoutStatus | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const filters: SessionFilters = {
        page,
        pageSize: PAGE_SIZE,
        ...(statusFilter ? { status: statusFilter } : {}),
      };
      const data = await listSessions(filters);
      setSessions(data.items);
      setTotalCount(data.totalCount);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load sessions");
    } finally {
      setLoading(false);
    }
  }, [page, statusFilter]);

  useEffect(() => {
    load();
  }, [load]);

  const handleSchedule = useCallback(
    async (req: ScheduleSessionRequest) => {
      await scheduleSSession(req);
      setPage(1);
      await load();
    },
    [load]
  );

  const handleDelete = useCallback(
    async (id: number) => {
      await deleteSession(id);
      await load();
    },
    [load]
  );

  const changeStatus = useCallback((status: WorkoutStatus | undefined) => {
    setStatusFilter(status);
    setPage(1);
  }, []);

  return {
    sessions,
    totalCount,
    page,
    pageSize: PAGE_SIZE,
    loading,
    error,
    statusFilter,
    setPage,
    setStatusFilter: changeStatus,
    scheduleSession: handleSchedule,
    deleteSession: handleDelete,
    refresh: load,
  };
}

export function useSessionDetail(id: number) {
  const [session, setSession] = useState<WorkoutSessionDetailResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getSession(id);
      setSession(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load session");
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  const handleUpdateStatus = useCallback(
    async (status: WorkoutStatus) => {
      if (!session) return;
      const updated = await updateSession(id, {
        title: session.title,
        scheduledAtUtc: session.scheduledAtUtc,
        comments: session.comments ?? undefined,
        status,
      });
      setSession(updated);
    },
    [id, session]
  );

  const handleComplete = useCallback(
    async (req: CompleteSessionRequest) => {
      const updated = await completeSession(id, req);
      setSession(updated);
    },
    [id]
  );

  return {
    session,
    loading,
    error,
    refresh: load,
    updateStatus: handleUpdateStatus,
    completeSession: handleComplete,
  };
}
