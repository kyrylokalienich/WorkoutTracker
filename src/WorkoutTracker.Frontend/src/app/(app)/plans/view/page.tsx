"use client";

import { Suspense, useState } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";
import Fab from "@mui/material/Fab";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import { PageHeader } from "@/components/ui/PageHeader";
import { LoadingSkeleton } from "@/components/ui/LoadingSkeleton";
import { EmptyState } from "@/components/ui/EmptyState";
import { PlanExerciseList } from "@/components/plans/PlanExerciseList";
import { CreateEditPlanDialog } from "@/components/plans/CreateEditPlanDialog";
import { AddExerciseDialog } from "@/components/plans/AddExerciseDialog";
import { usePlanDetail } from "@/hooks/useWorkoutPlans";
import { updatePlan as apiUpdatePlan } from "@/lib/api/plans";
import type { CreatePlanRequest } from "@/types/plan";

function PlanDetailContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const planId = Number(searchParams.get("id"));

  const { plan, loading, error, refresh, addExercise, removeExercise } =
    usePlanDetail(planId);

  const [editOpen, setEditOpen] = useState(false);
  const [addExerciseOpen, setAddExerciseOpen] = useState(false);

  const handleUpdatePlan = async (req: CreatePlanRequest) => {
    await apiUpdatePlan(planId, req);
    await refresh();
  };

  if (loading) {
    return (
      <Box className="px-4 py-6 max-w-3xl mx-auto w-full">
        <LoadingSkeleton count={1} variant="card" />
      </Box>
    );
  }

  if (error || !plan) {
    return (
      <Box className="px-4 py-6 max-w-3xl mx-auto w-full">
        <Alert severity="error">{error ?? "Plan not found"}</Alert>
      </Box>
    );
  }

  return (
    <Box className="px-4 py-6 max-w-3xl mx-auto w-full">
      <PageHeader
        title={plan.name}
        action={
          <Box sx={{ display: "flex", gap: 1 }}>
            <Button
              startIcon={<ArrowBackIcon />}
              onClick={() => router.push("/plans")}
              size="small"
            >
              Plans
            </Button>
            <Button
              startIcon={<EditIcon />}
              onClick={() => setEditOpen(true)}
              variant="outlined"
              size="small"
            >
              Edit
            </Button>
          </Box>
        }
      />

      <Box sx={{ display: "flex", gap: 1, mb: 2, flexWrap: "wrap" }}>
        <Chip
          label={plan.isActive ? "Active" : "Inactive"}
          color={plan.isActive ? "success" : "default"}
          size="small"
        />
        <Chip
          label={`${plan.exercises.length} exercise${plan.exercises.length !== 1 ? "s" : ""}`}
          size="small"
          variant="outlined"
        />
      </Box>

      {plan.description && (
        <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
          {plan.description}
        </Typography>
      )}

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Exercises
          </Typography>
          {plan.exercises.length === 0 ? (
            <EmptyState
              icon={<FitnessCenterIcon sx={{ fontSize: 48 }} />}
              message="No exercises yet"
              description="Add exercises to build your plan"
              actionLabel="Add exercise"
              onAction={() => setAddExerciseOpen(true)}
            />
          ) : (
            <PlanExerciseList exercises={plan.exercises} onRemove={removeExercise} />
          )}
        </CardContent>
      </Card>

      <Fab
        color="primary"
        aria-label="Add exercise"
        onClick={() => setAddExerciseOpen(true)}
        sx={{ position: "fixed", bottom: { xs: 80, sm: 24 }, right: 24 }}
      >
        <AddIcon />
      </Fab>

      <CreateEditPlanDialog
        open={editOpen}
        plan={plan}
        onClose={() => setEditOpen(false)}
        onSave={handleUpdatePlan}
      />

      <AddExerciseDialog
        open={addExerciseOpen}
        existingExerciseIds={plan.exercises.map((e) => e.exerciseId)}
        nextOrderIndex={plan.exercises.length}
        onClose={() => setAddExerciseOpen(false)}
        onAdd={addExercise}
      />
    </Box>
  );
}

export default function PlanDetailPage() {
  return (
    <Suspense fallback={null}>
      <PlanDetailContent />
    </Suspense>
  );
}
