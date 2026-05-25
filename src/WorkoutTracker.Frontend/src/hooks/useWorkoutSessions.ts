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
import {
  WorkoutStatus,
  type WorkoutSessionSummaryResponse,
  type WorkoutSessionDetailResponse,
  type ScheduleSessionRequest,
  type StartNowSessionRequest,
  type CompleteSessionRequest,
  type SessionFilters,
} from "@/types/session";

const PAGE_SIZE = 10;

export function useWorkoutSessions() {
  const [sessions, setSessions] = useState<WorkoutSessionSummaryResponse[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<WorkoutStatus | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Tracks which card's action button is in flight (by session id)
  const [actionLoading, setActionLoading] = useState<number | null>(null);
  // Holds the detail needed to show the Finish / CompleteSessionDialog
  const [sessionToComplete, setSessionToComplete] = useState<WorkoutSessionDetailResponse | null>(null);

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

  /** Transition a Planned session to InProgress. Fetches detail first to preserve comments. */
  const handleStart = useCallback(
    async (id: number) => {
      setActionLoading(id);
      setError(null);
      try {
        const detail = await getSession(id);
        await updateSession(id, {
          title: detail.title,
          scheduledAtUtc: detail.scheduledAtUtc,
          comments: detail.comments ?? undefined,
          status: WorkoutStatus.InProgress,
        });
        await load();
      } catch (e) {
        setError(e instanceof Error ? e.message : "Failed to start session");
      } finally {
        setActionLoading(null);
      }
    },
    [load]
  );

  /** Fetch detail and open the CompleteSessionDialog for an InProgress session. */
  const handleOpenFinish = useCallback(async (id: number) => {
    setActionLoading(id);
    setError(null);
    try {
      const detail = await getSession(id);
      setSessionToComplete(detail);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load session");
    } finally {
      setActionLoading(null);
    }
  }, []);

  /** Called by CompleteSessionDialog on submit. */
  const handleFinish = useCallback(
    async (req: CompleteSessionRequest) => {
      if (!sessionToComplete) return;
      await completeSession(sessionToComplete.id, req);
      setSessionToComplete(null);
      await load();
    },
    [sessionToComplete, load]
  );

  const handleCloseFinish = useCallback(() => setSessionToComplete(null), []);

  /** Creates a session scheduled for now and immediately transitions it to InProgress. Returns the new session id. */
  const handleStartNow = useCallback(
    async (req: StartNowSessionRequest): Promise<number> => {
      const created = await scheduleSSession({
        title: req.title,
        scheduledAtUtc: new Date().toISOString(),
        ...(req.workoutPlanId ? { workoutPlanId: req.workoutPlanId } : {}),
      });
      await updateSession(created.id, {
        title: created.title,
        scheduledAtUtc: created.scheduledAtUtc,
        status: WorkoutStatus.InProgress,
      });
      return created.id;
    },
    []
  );

  return {
    sessions,
    totalCount,
    page,
    pageSize: PAGE_SIZE,
    loading,
    error,
    statusFilter,
    actionLoading,
    sessionToComplete,
    setPage,
    setStatusFilter: changeStatus,
    scheduleSession: handleSchedule,
    deleteSession: handleDelete,
    startSession: handleStart,
    openFinishDialog: handleOpenFinish,
    finishSession: handleFinish,
    closeFinishDialog: handleCloseFinish,
    startNowSession: handleStartNow,
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
