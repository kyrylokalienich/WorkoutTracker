"use client";

import { useState, useEffect, useMemo } from "react";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import Alert from "@mui/material/Alert";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import InputAdornment from "@mui/material/InputAdornment";
import SearchIcon from "@mui/icons-material/Search";
import { listExercises } from "@/lib/api/exercises";
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
  const [allExercises, setAllExercises] = useState<ExerciseResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<ExerciseCategory | undefined>();
  const [muscleFilter, setMuscleFilter] = useState<MuscleGroup | undefined>();

  // Fetch the full catalogue once on mount — filtering is done client-side.
  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    listExercises()
      .then((data) => { if (!cancelled) { setAllExercises(data); setLoading(false); } })
      .catch((e) => { if (!cancelled) { setError(e instanceof Error ? e.message : "Failed to load exercises"); setLoading(false); } });
    return () => { cancelled = true; };
  }, []);

  const available = useMemo(() => {
    const q = search.trim().toLowerCase();
    return allExercises.filter((e) => {
      if (existingExerciseIds.includes(e.id)) return false;
      if (categoryFilter && e.category !== categoryFilter) return false;
      if (muscleFilter && e.muscleGroup !== muscleFilter) return false;
      if (q && !e.name.toLowerCase().includes(q)) return false;
      return true;
    });
  }, [allExercises, existingExerciseIds, search, categoryFilter, muscleFilter]);

  return (
    <>
      <DialogContent>
        <TextField
          placeholder="Search by name…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          fullWidth
          size="small"
          autoFocus
          sx={{ mb: 2 }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" />
              </InputAdornment>
            ),
          }}
        />

        {/* Category filter */}
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

        {/* Muscle-group filter — only shown when a category is active */}
        {categoryFilter && (
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
        )}

        {!categoryFilter && <Box sx={{ mb: 2 }} />}

        {error ? (
          <Alert severity="error">{error}</Alert>
        ) : loading ? (
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
