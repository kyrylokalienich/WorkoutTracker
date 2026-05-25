"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import { ExercisePickerStep } from "@/components/ui/ExercisePickerStep";
import type { ExerciseResponse } from "@/types/exercise";
import type { AddSessionExerciseRequest } from "@/types/session";

const configSchema = z.object({
  plannedSets: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  plannedReps: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  plannedWeightKg: z.number().min(0, "Min 0").optional(),
});

type ConfigData = z.infer<typeof configSchema>;

interface AddSessionExerciseDialogProps {
  open: boolean;
  existingExerciseIds: number[];
  onClose: () => void;
  onAdd: (req: AddSessionExerciseRequest) => Promise<void>;
}

export function AddSessionExerciseDialog({
  open,
  existingExerciseIds,
  onClose,
  onAdd,
}: AddSessionExerciseDialogProps) {
  const [selected, setSelected] = useState<ExerciseResponse | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ConfigData>({
    resolver: zodResolver(configSchema),
    defaultValues: { plannedSets: 3, plannedReps: 10 },
  });

  const handleClose = () => {
    setSelected(null);
    reset();
    onClose();
  };

  const onSubmit = async (data: ConfigData) => {
    if (!selected) return;
    await onAdd({
      exerciseId: selected.id,
      plannedSets: data.plannedSets,
      plannedReps: data.plannedReps,
      plannedWeightKg: data.plannedWeightKg,
    });
    handleClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {selected ? `Configure: ${selected.name}` : "Add exercise"}
      </DialogTitle>

      {!selected ? (
        <ExercisePickerStep
          existingExerciseIds={existingExerciseIds}
          onSelect={setSelected}
          onCancel={handleClose}
        />
      ) : (
        <Box
          component="form"
          onSubmit={(e: React.FormEvent) => { e.stopPropagation(); handleSubmit(onSubmit)(e); }}
        >
          <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Typography variant="body2" color="text.secondary">
              {selected.category} · {selected.muscleGroup}
            </Typography>
            <TextField
              label="Planned sets"
              type="number"
              {...register("plannedSets", { valueAsNumber: true })}
              error={!!errors.plannedSets}
              helperText={errors.plannedSets?.message}
              inputProps={{ min: 1 }}
              fullWidth
            />
            <TextField
              label="Planned reps"
              type="number"
              {...register("plannedReps", { valueAsNumber: true })}
              error={!!errors.plannedReps}
              helperText={errors.plannedReps?.message}
              inputProps={{ min: 1 }}
              fullWidth
            />
            <TextField
              label="Planned weight (kg) — optional"
              type="number"
              {...register("plannedWeightKg", { valueAsNumber: true })}
              error={!!errors.plannedWeightKg}
              helperText={errors.plannedWeightKg?.message}
              inputProps={{ min: 0, step: 0.5 }}
              fullWidth
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setSelected(null)} disabled={isSubmitting}>
              Back
            </Button>
            <Button type="submit" variant="contained" disabled={isSubmitting}>
              {isSubmitting ? <CircularProgress size={20} color="inherit" /> : "Add"}
            </Button>
          </DialogActions>
        </Box>
      )}
    </Dialog>
  );
}
