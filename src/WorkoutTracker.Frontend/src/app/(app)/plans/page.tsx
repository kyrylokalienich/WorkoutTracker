"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import Alert from "@mui/material/Alert";
import AddIcon from "@mui/icons-material/Add";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { LoadingSkeleton } from "@/components/ui/LoadingSkeleton";
import { PlanCard } from "@/components/plans/PlanCard";
import { CreateEditPlanDialog } from "@/components/plans/CreateEditPlanDialog";
import { useWorkoutPlans } from "@/hooks/useWorkoutPlans";
import type { WorkoutPlanSummaryResponse } from "@/types/plan";

export default function PlansPage() {
  const router = useRouter();
  const { plans, loading, error, createPlan, updatePlan, deletePlan } = useWorkoutPlans();
  const [createOpen, setCreateOpen] = useState(false);
  const [editTarget, setEditTarget] = useState<WorkoutPlanSummaryResponse | null>(null);

  if (loading) {
    return (
      <Box className="px-4 py-6 max-w-5xl mx-auto w-full">
        <PageHeader title="Workout Plans" />
        <LoadingSkeleton count={4} variant="card" />
      </Box>
    );
  }

  return (
    <Box className="px-4 py-6 max-w-5xl mx-auto w-full">
      <PageHeader title="Workout Plans" />

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {plans.length === 0 ? (
        <EmptyState
          icon={<FitnessCenterIcon sx={{ fontSize: 64 }} />}
          message="No workout plans yet"
          description="Create your first plan to get started"
          actionLabel="Create plan"
          onAction={() => setCreateOpen(true)}
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {plans.map((plan) => (
            <PlanCard
              key={plan.id}
              plan={plan}
              onClick={() => router.push(`/plans/view?id=${plan.id}`)}
              onEdit={() => setEditTarget(plan)}
              onDelete={() => deletePlan(plan.id)}
            />
          ))}
        </div>
      )}

      <Fab
        color="primary"
        aria-label="Create plan"
        onClick={() => setCreateOpen(true)}
        sx={{ position: "fixed", bottom: { xs: 80, sm: 24 }, right: 24 }}
      >
        <AddIcon />
      </Fab>

      <CreateEditPlanDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSave={createPlan}
      />

      <CreateEditPlanDialog
        open={!!editTarget}
        plan={editTarget}
        onClose={() => setEditTarget(null)}
        onSave={(req) => updatePlan(editTarget!.id, req)}
      />
    </Box>
  );
}
