"use client";

import { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import InputAdornment from "@mui/material/InputAdornment";
import SearchIcon from "@mui/icons-material/Search";
import { useExercises } from "@/hooks/useExercises";
import { ExerciseCategory, MuscleGroup } from "@/types/exercise";
import type { ExerciseResponse } from "@/types/exercise";
import type { AddPlanExerciseRequest } from "@/types/plan";

const configSchema = z.object({
  targetSets: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  targetReps: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  targetWeightKg: z.number().min(0, "Min 0").optional(),
});

type ConfigData = z.infer<typeof configSchema>;

interface AddExerciseDialogProps {
  open: boolean;
  existingExerciseIds: number[];
  nextOrderIndex: number;
  onClose: () => void;
  onAdd: (req: AddPlanExerciseRequest) => Promise<void>;
}

export function AddExerciseDialog({
  open,
  existingExerciseIds,
  nextOrderIndex,
  onClose,
  onAdd,
}: AddExerciseDialogProps) {
  const [selected, setSelected] = useState<ExerciseResponse | null>(null);
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<ExerciseCategory | undefined>();
  const [muscleFilter, setMuscleFilter] = useState<MuscleGroup | undefined>();

  const { exercises, loading } = useExercises({
    search: search || undefined,
    category: categoryFilter,
    muscleGroup: muscleFilter,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ConfigData>({
    resolver: zodResolver(configSchema),
    defaultValues: { targetSets: 3, targetReps: 10 },
  });

  const available = exercises.filter((e) => !existingExerciseIds.includes(e.id));

  const handleClose = () => {
    setSelected(null);
    setSearch("");
    setCategoryFilter(undefined);
    setMuscleFilter(undefined);
    reset();
    onClose();
  };

  const onSubmit = async (data: ConfigData) => {
    if (!selected) return;
    await onAdd({
      exerciseId: selected.id,
      targetSets: data.targetSets,
      targetReps: data.targetReps,
      targetWeightKg: data.targetWeightKg,
      orderIndex: nextOrderIndex,
    });
    handleClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {selected ? `Configure: ${selected.name}` : "Add exercise"}
      </DialogTitle>

      {!selected ? (
        <>
          <DialogContent>
            <TextField
              placeholder="Search exercises..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              fullWidth
              size="small"
              sx={{ mb: 2 }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon fontSize="small" />
                  </InputAdornment>
                ),
              }}
            />

            <Box sx={{ display: "flex", gap: 0.5, flexWrap: "wrap", mb: 1 }}>
              {Object.values(ExerciseCategory).map((cat) => (
                <Chip
                  key={cat}
                  label={cat}
                  size="small"
                  variant={categoryFilter === cat ? "filled" : "outlined"}
                  color={categoryFilter === cat ? "primary" : "default"}
                  onClick={() =>
                    setCategoryFilter(categoryFilter === cat ? undefined : cat)
                  }
                />
              ))}
            </Box>

            <Box sx={{ display: "flex", gap: 0.5, flexWrap: "wrap", mb: 2 }}>
              {Object.values(MuscleGroup).map((mg) => (
                <Chip
                  key={mg}
                  label={mg}
                  size="small"
                  variant={muscleFilter === mg ? "filled" : "outlined"}
                  color={muscleFilter === mg ? "secondary" : "default"}
                  onClick={() =>
                    setMuscleFilter(muscleFilter === mg ? undefined : mg)
                  }
                />
              ))}
            </Box>

            {loading ? (
              <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
                <CircularProgress size={32} />
              </Box>
            ) : available.length === 0 ? (
              <Typography color="text.secondary" textAlign="center" sx={{ py: 4 }}>
                No exercises found
              </Typography>
            ) : (
              <List dense sx={{ maxHeight: 300, overflow: "auto" }}>
                {available.map((ex) => (
                  <ListItem key={ex.id} disablePadding>
                    <ListItemButton onClick={() => setSelected(ex)}>
                      <ListItemText
                        primary={ex.name}
                        secondary={`${ex.category} · ${ex.muscleGroup}`}
                      />
                    </ListItemButton>
                  </ListItem>
                ))}
              </List>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancel</Button>
          </DialogActions>
        </>
      ) : (
        <Box component="form" onSubmit={handleSubmit(onSubmit)}>
          <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Typography variant="body2" color="text.secondary">
              {selected.category} · {selected.muscleGroup}
            </Typography>

            <TextField
              label="Sets"
              type="number"
              {...register("targetSets", { valueAsNumber: true })}
              error={!!errors.targetSets}
              helperText={errors.targetSets?.message}
              inputProps={{ min: 1 }}
              fullWidth
            />
            <TextField
              label="Reps"
              type="number"
              {...register("targetReps", { valueAsNumber: true })}
              error={!!errors.targetReps}
              helperText={errors.targetReps?.message}
              inputProps={{ min: 1 }}
              fullWidth
            />
            <TextField
              label="Weight (kg) — optional"
              type="number"
              {...register("targetWeightKg", { valueAsNumber: true })}
              error={!!errors.targetWeightKg}
              helperText={errors.targetWeightKg?.message}
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
