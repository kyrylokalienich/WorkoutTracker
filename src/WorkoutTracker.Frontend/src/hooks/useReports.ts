"use client";

import { useCallback, useEffect, useState } from "react";
import {
  getWorkoutHistory,
  getProgressReport,
  getMuscleGroupVolume,
} from "@/lib/api/reports";
import type {
  WorkoutHistoryResponse,
  ProgressReportResponse,
  MuscleGroupVolumeResponse,
  DateRangeFilter,
} from "@/types/report";

function defaultDateRange(): DateRangeFilter {
  const to = new Date();
  const from = new Date();
  from.setDate(from.getDate() - 30);
  return {
    from: from.toISOString().slice(0, 10),
    to: to.toISOString().slice(0, 10),
  };
}

export function useReports() {
  const [history, setHistory] = useState<WorkoutHistoryResponse | null>(null);
  const [progress, setProgress] = useState<ProgressReportResponse | null>(null);
  const [muscleGroups, setMuscleGroups] = useState<MuscleGroupVolumeResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dateRange, setDateRange] = useState<DateRangeFilter>(defaultDateRange);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [h, p, m] = await Promise.all([
        getWorkoutHistory(dateRange),
        getProgressReport(dateRange),
        getMuscleGroupVolume(dateRange),
      ]);
      setHistory(h);
      setProgress(p);
      setMuscleGroups(m);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load reports");
    } finally {
      setLoading(false);
    }
  }, [dateRange]);

  useEffect(() => {
    load();
  }, [load]);

  return { history, progress, muscleGroups, loading, error, dateRange, setDateRange };
}
