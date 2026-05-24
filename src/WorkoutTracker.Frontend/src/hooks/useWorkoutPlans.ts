"use client";

import { useCallback, useEffect, useState } from "react";
import {
  listPlans,
  getPlan,
  createPlan,
  updatePlan,
  deletePlan,
  addPlanExercise,
  updatePlanExercise,
  deletePlanExercise,
} from "@/lib/api/plans";
import type {
  WorkoutPlanSummaryResponse,
  WorkoutPlanDetailResponse,
  CreatePlanRequest,
  UpdatePlanRequest,
  AddPlanExerciseRequest,
  UpdatePlanExerciseRequest,
} from "@/types/plan";

export function useWorkoutPlans() {
  const [plans, setPlans] = useState<WorkoutPlanSummaryResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await listPlans();
      setPlans(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load plans");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const handleCreate = useCallback(
    async (req: CreatePlanRequest) => {
      await createPlan(req);
      await load();
    },
    [load]
  );

  const handleUpdate = useCallback(
    async (id: number, req: UpdatePlanRequest) => {
      await updatePlan(id, req);
      await load();
    },
    [load]
  );

  const handleDelete = useCallback(
    async (id: number) => {
      await deletePlan(id);
      await load();
    },
    [load]
  );

  return {
    plans,
    loading,
    error,
    refresh: load,
    createPlan: handleCreate,
    updatePlan: handleUpdate,
    deletePlan: handleDelete,
  };
}

export function usePlanDetail(id: number) {
  const [plan, setPlan] = useState<WorkoutPlanDetailResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getPlan(id);
      setPlan(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load plan");
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    load();
  }, [load]);

  const handleAddExercise = useCallback(
    async (req: AddPlanExerciseRequest) => {
      await addPlanExercise(id, req);
      await load();
    },
    [id, load]
  );

  const handleUpdateExercise = useCallback(
    async (planExerciseId: number, req: UpdatePlanExerciseRequest) => {
      await updatePlanExercise(id, planExerciseId, req);
      await load();
    },
    [id, load]
  );

  const handleRemoveExercise = useCallback(
    async (planExerciseId: number) => {
      await deletePlanExercise(id, planExerciseId);
      await load();
    },
    [id, load]
  );

  return {
    plan,
    loading,
    error,
    refresh: load,
    addExercise: handleAddExercise,
    updateExercise: handleUpdateExercise,
    removeExercise: handleRemoveExercise,
  };
}
