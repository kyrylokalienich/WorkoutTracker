"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Divider from "@mui/material/Divider";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import type { WorkoutSessionDetailResponse, CompleteSessionRequest } from "@/types/session";

const exerciseSchema = z.object({
  actualSets: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  actualReps: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  actualWeightKg: z.number().min(0).optional(),
  notes: z.string().max(500).optional(),
});

function buildSchema(count: number) {
  return z.object(
    Object.fromEntries(
      Array.from({ length: count }, (_, i) => [String(i), exerciseSchema])
    )
  );
}

type FormData = Record<string, {
  actualSets: number;
  actualReps: number;
  actualWeightKg?: number;
  notes?: string;
}>;

interface CompleteSessionDialogProps {
  open: boolean;
  session: WorkoutSessionDetailResponse;
  onClose: () => void;
  onComplete: (req: CompleteSessionRequest) => Promise<void>;
}

export function CompleteSessionDialog({
  open,
  session,
  onClose,
  onComplete,
}: CompleteSessionDialogProps) {
  const schema = buildSchema(session.exercises.length);

  const defaultValues = Object.fromEntries(
    session.exercises.map((ex, i) => [
      String(i),
      {
        actualSets: ex.plannedSets,
        actualReps: ex.plannedReps,
        actualWeightKg: ex.plannedWeightKg ?? undefined,
        notes: "",
      },
    ])
  );

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues,
  });

  const onSubmit = async (data: FormData) => {
    const req: CompleteSessionRequest = {
      exercises: session.exercises.map((ex, i) => ({
        sessionExerciseId: ex.id,
        actualSets: data[String(i)].actualSets,
        actualReps: data[String(i)].actualReps,
        actualWeightKg: data[String(i)].actualWeightKg,
        notes: data[String(i)].notes?.trim() || undefined,
      })),
    };
    await onComplete(req);
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Complete session</DialogTitle>
      <Box component="form" onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
          {session.exercises.map((ex, i) => {
            const rowErrors = (errors as Record<string, Record<string, { message?: string }>>)[String(i)];
            return (
              <Box key={ex.id}>
                {i > 0 && <Divider sx={{ mb: 2 }} />}
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  {ex.exerciseName}
                </Typography>
                <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: "block" }}>
                  Planned: {ex.plannedSets} sets × {ex.plannedReps} reps
                  {ex.plannedWeightKg != null ? ` @ ${ex.plannedWeightKg} kg` : ""}
                </Typography>
                <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                  <TextField
                    label="Actual sets"
                    type="number"
                    {...register(`${i}.actualSets` as keyof FormData, { valueAsNumber: true })}
                    error={!!rowErrors?.actualSets}
                    helperText={rowErrors?.actualSets?.message}
                    inputProps={{ min: 1 }}
                    sx={{ flex: 1, minWidth: 90 }}
                  />
                  <TextField
                    label="Actual reps"
                    type="number"
                    {...register(`${i}.actualReps` as keyof FormData, { valueAsNumber: true })}
                    error={!!rowErrors?.actualReps}
                    helperText={rowErrors?.actualReps?.message}
                    inputProps={{ min: 1 }}
                    sx={{ flex: 1, minWidth: 90 }}
                  />
                  <TextField
                    label="Weight (kg)"
                    type="number"
                    {...register(`${i}.actualWeightKg` as keyof FormData, { valueAsNumber: true })}
                    error={!!rowErrors?.actualWeightKg}
                    helperText={rowErrors?.actualWeightKg?.message}
                    inputProps={{ min: 0, step: 0.5 }}
                    sx={{ flex: 1, minWidth: 90 }}
                  />
                </Box>
                <TextField
                  label="Notes (optional)"
                  {...register(`${i}.notes` as keyof FormData)}
                  fullWidth
                  size="small"
                  sx={{ mt: 1 }}
                  multiline
                  rows={1}
                />
              </Box>
            );
          })}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" color="success" disabled={isSubmitting}>
            {isSubmitting ? <CircularProgress size={20} color="inherit" /> : "Complete"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
