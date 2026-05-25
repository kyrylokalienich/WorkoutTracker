"use client";

import { useState } from "react";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import InputAdornment from "@mui/material/InputAdornment";
import SearchIcon from "@mui/icons-material/Search";
import { useExercises } from "@/hooks/useExercises";
import { ExerciseCategory, MuscleGroup } from "@/types/exercise";
import type { ExerciseResponse } from "@/types/exercise";

interface ExercisePickerStepProps {
  existingExerciseIds: number[];
  onSelect: (ex: ExerciseResponse) => void;
  onCancel: () => void;
}

export function ExercisePickerStep({
  existingExerciseIds,
  onSelect,
  onCancel,
}: ExercisePickerStepProps) {
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<ExerciseCategory | undefined>();
  const [muscleFilter, setMuscleFilter] = useState<MuscleGroup | undefined>();

  const { exercises, loading } = useExercises({
    search: search || undefined,
    category: categoryFilter,
    muscleGroup: muscleFilter,
  });

  const available = exercises.filter((e) => !existingExerciseIds.includes(e.id));

  return (
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
              onClick={() => setCategoryFilter(categoryFilter === cat ? undefined : cat)}
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
              onClick={() => setMuscleFilter(muscleFilter === mg ? undefined : mg)}
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
                <ListItemButton onClick={() => onSelect(ex)}>
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
        <Button onClick={onCancel}>Cancel</Button>
      </DialogActions>
    </>
  );
}
