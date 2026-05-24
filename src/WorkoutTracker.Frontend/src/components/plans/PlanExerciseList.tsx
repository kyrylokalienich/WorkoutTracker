"use client";

import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import Typography from "@mui/material/Typography";
import Chip from "@mui/material/Chip";
import Box from "@mui/material/Box";
import DeleteIcon from "@mui/icons-material/Delete";
import type { WorkoutPlanExerciseResponse } from "@/types/plan";

interface PlanExerciseListProps {
  exercises: WorkoutPlanExerciseResponse[];
  onRemove: (planExerciseId: number) => Promise<void>;
}

export function PlanExerciseList({ exercises, onRemove }: PlanExerciseListProps) {
  if (exercises.length === 0) return null;

  return (
    <List disablePadding>
      {exercises
        .slice()
        .sort((a, b) => a.orderIndex - b.orderIndex)
        .map((ex, index) => (
          <ListItem
            key={ex.id}
            divider={index < exercises.length - 1}
            secondaryAction={
              <Tooltip title="Remove exercise">
                <IconButton
                  edge="end"
                  size="small"
                  color="error"
                  onClick={() => onRemove(ex.id)}
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            }
            sx={{ pr: 6 }}
          >
            <ListItemText
              primary={ex.exerciseName}
              secondaryTypographyProps={{ component: "div" }}
              secondary={
                <Box sx={{ display: "flex", gap: 0.5, flexWrap: "wrap", mt: 0.5 }}>
                  <Chip
                    label={`${ex.targetSets} sets`}
                    size="small"
                    variant="outlined"
                  />
                  <Chip
                    label={`${ex.targetReps} reps`}
                    size="small"
                    variant="outlined"
                  />
                  {ex.targetWeightKg != null && (
                    <Chip
                      label={`${ex.targetWeightKg} kg`}
                      size="small"
                      variant="outlined"
                    />
                  )}
                </Box>
              }
            />
          </ListItem>
        ))}
    </List>
  );
}
