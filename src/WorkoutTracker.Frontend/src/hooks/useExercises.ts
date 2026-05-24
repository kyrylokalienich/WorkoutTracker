"use client";

import { useCallback, useEffect, useState } from "react";
import { listExercises } from "@/lib/api/exercises";
import type { ExerciseResponse, ExerciseFilters } from "@/types/exercise";

export function useExercises(initialFilters: ExerciseFilters = {}) {
  const [exercises, setExercises] = useState<ExerciseResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<ExerciseFilters>(initialFilters);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await listExercises(filters);
      setExercises(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load exercises");
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    load();
  }, [load]);

  return { exercises, loading, error, filters, setFilters };
}
