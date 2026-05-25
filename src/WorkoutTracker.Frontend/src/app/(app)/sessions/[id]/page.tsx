"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useParams, useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";
import Chip from "@mui/material/Chip";
import Divider from "@mui/material/Divider";
import TextField from "@mui/material/TextField";
import CircularProgress from "@mui/material/CircularProgress";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import CheckIcon from "@mui/icons-material/Check";
import BlockIcon from "@mui/icons-material/Block";
import TimerIcon from "@mui/icons-material/Timer";
import FlagIcon from "@mui/icons-material/Flag";
import { PageHeader } from "@/components/ui/PageHeader";
import { LoadingSkeleton } from "@/components/ui/LoadingSkeleton";
import { SessionStatusChip } from "@/components/sessions/SessionStatusChip";
import { useSessionDetail } from "@/hooks/useWorkoutSessions";
import { WorkoutStatus } from "@/types/session";
import type { CompleteSessionRequest } from "@/types/session";
import { formatDuration, formatShortTime } from "@/lib/utils/time";


const exerciseRowSchema = z.object({
  actualSets: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  actualReps: z.number({ invalid_type_error: "Required" }).int().min(1, "Min 1"),
  actualWeightKg: z.number().min(0).optional(),
  notes: z.string().max(500).optional(),
});

const formSchema = z.object({
  comments: z.string().max(2000).optional(),
  exercises: z.array(exerciseRowSchema),
});

type FormData = z.infer<typeof formSchema>;


function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}


export default function SessionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const sessionId = Number(id);

  const { session, loading, error, updateStatus, completeSession } =
    useSessionDetail(sessionId);

  // Live elapsed timer — ticks every second while session is InProgress
  const [elapsed, setElapsed] = useState<string>("");
  useEffect(() => {
    if (session?.status !== WorkoutStatus.InProgress || !session.startedAtUtc) {
      setElapsed("");
      return;
    }
    const tick = () => setElapsed(formatDuration(session.startedAtUtc!));
    tick();
    const timerId = setInterval(tick, 1000);
    return () => clearInterval(timerId);
  }, [session?.status, session?.startedAtUtc]);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: { comments: "", exercises: [] },
  });

  useEffect(() => {
    if (!session) return;
    reset({
      comments: session.comments ?? "",
      exercises: session.exercises.map((ex) => ({
        actualSets: ex.actualSets ?? ex.plannedSets,
        actualReps: ex.actualReps ?? ex.plannedReps,
        actualWeightKg: ex.actualWeightKg ?? ex.plannedWeightKg ?? undefined,
        notes: ex.notes ?? "",
      })),
    });
  }, [session, reset]);

  // ── submit ─────────────────────────────────────────────────────────────────
  const onComplete = async (data: FormData) => {
    if (!session) return;
    const req: CompleteSessionRequest = {
      comments: data.comments?.trim() || undefined,
      exercises: session.exercises.map((ex, i) => ({
        sessionExerciseId: ex.id,
        actualSets: data.exercises[i].actualSets,
        actualReps: data.exercises[i].actualReps,
        actualWeightKg: data.exercises[i].actualWeightKg,
        notes: data.exercises[i].notes?.trim() || undefined,
      })),
    };
    await completeSession(req);
  };

  if (loading) {
    return (
      <Box className="px-4 py-6 max-w-3xl mx-auto w-full">
        <LoadingSkeleton count={1} variant="card" />
      </Box>
    );
  }

  if (error || !session) {
    return (
      <Box className="px-4 py-6 max-w-3xl mx-auto w-full">
        <Alert severity="error">{error ?? "Session not found"}</Alert>
      </Box>
    );
  }

  const isPlanned = session.status === WorkoutStatus.Planned;
  const isInProgress = session.status === WorkoutStatus.InProgress;

  return (
    <Box
      component={isInProgress ? "form" : "div"}
      onSubmit={isInProgress ? handleSubmit(onComplete) : undefined}
      className="px-4 py-6 max-w-3xl mx-auto w-full"
    >
      <PageHeader
        title={session.title}
        action={
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => router.push("/sessions")}
            size="small"
          >
            Sessions
          </Button>
        }
      />

      {/* Status + scheduled date */}
      <Box sx={{ display: "flex", gap: 1, mb: 2, flexWrap: "wrap", alignItems: "center" }}>
        <SessionStatusChip status={session.status} size="medium" />
        <Typography variant="caption" color="text.secondary">
          Scheduled: {formatDate(session.scheduledAtUtc)}
        </Typography>
      </Box>

      {/* Timestamp + duration row */}
      {(session.startedAtUtc || session.completedAtUtc) && (
        <Box sx={{ display: "flex", gap: 2, mb: 3, flexWrap: "wrap", alignItems: "center" }}>
          {session.startedAtUtc && (
            <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
              <PlayArrowIcon fontSize="small" color="action" />
              <Typography variant="body2" color="text.secondary">
                Started {formatShortTime(session.startedAtUtc)}
              </Typography>
            </Box>
          )}
          {session.completedAtUtc && (
            <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
              <FlagIcon fontSize="small" color="success" />
              <Typography variant="body2" color="text.secondary">
                Finished {formatShortTime(session.completedAtUtc)}
              </Typography>
            </Box>
          )}
          {isInProgress && elapsed && (
            <Chip
              icon={<TimerIcon />}
              label={elapsed}
              color="warning"
              size="small"
              variant="outlined"
            />
          )}
          {session.status === WorkoutStatus.Completed &&
            session.startedAtUtc &&
            session.completedAtUtc && (
              <Chip
                icon={<TimerIcon />}
                label={formatDuration(session.startedAtUtc, session.completedAtUtc)}
                color="success"
                size="small"
                variant="outlined"
              />
            )}
        </Box>
      )}

      {/* Comments — editable when InProgress, read-only otherwise */}
      {isInProgress ? (
        <TextField
          label="Session notes"
          {...register("comments")}
          fullWidth
          multiline
          minRows={2}
          size="small"
          placeholder="How did it go? Any notes for this session…"
          sx={{ mb: 3 }}
        />
      ) : (
        session.comments && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {session.comments}
          </Typography>
        )
      )}

      {/* Action buttons */}
      <Box sx={{ display: "flex", gap: 1, mb: 3, flexWrap: "wrap" }}>
        {isPlanned && (
          <>
            <Button
              variant="contained"
              startIcon={<PlayArrowIcon />}
              onClick={() => updateStatus(WorkoutStatus.InProgress)}
            >
              Start
            </Button>
            <Button
              variant="outlined"
              color="warning"
              startIcon={<BlockIcon />}
              onClick={() => updateStatus(WorkoutStatus.Skipped)}
            >
              Skip
            </Button>
          </>
        )}
        {isInProgress && (
          <Button
            type="submit"
            variant="contained"
            color="success"
            startIcon={isSubmitting ? undefined : <CheckIcon />}
            disabled={isSubmitting}
          >
            {isSubmitting ? <CircularProgress size={20} color="inherit" /> : "Complete Workout"}
          </Button>
        )}
      </Box>

      {/* ── Exercise list ──────────────────────────────────────────────────── */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Exercises ({session.exercises.length})
          </Typography>

          {isInProgress ? (
            /* Editable exercise cards */
            <Box sx={{ display: "flex", flexDirection: "column", gap: 0 }}>
              {session.exercises.map((ex, i) => {
                const rowErrors = errors.exercises?.[i];
                return (
                  <Box key={ex.id}>
                    {i > 0 && <Divider sx={{ my: 2 }} />}
                    <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                      {ex.exerciseName}
                    </Typography>
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{ mb: 1.5, display: "block" }}
                    >
                      Planned: {ex.plannedSets} sets × {ex.plannedReps} reps
                      {ex.plannedWeightKg != null ? ` @ ${ex.plannedWeightKg} kg` : ""}
                    </Typography>
                    <Box sx={{ display: "flex", gap: 1, flexWrap: "wrap" }}>
                      <TextField
                        label="Actual sets"
                        type="number"
                        {...register(`exercises.${i}.actualSets`, { valueAsNumber: true })}
                        error={!!rowErrors?.actualSets}
                        helperText={rowErrors?.actualSets?.message}
                        inputProps={{ min: 1 }}
                        sx={{ flex: 1, minWidth: 90 }}
                        size="small"
                      />
                      <TextField
                        label="Actual reps"
                        type="number"
                        {...register(`exercises.${i}.actualReps`, { valueAsNumber: true })}
                        error={!!rowErrors?.actualReps}
                        helperText={rowErrors?.actualReps?.message}
                        inputProps={{ min: 1 }}
                        sx={{ flex: 1, minWidth: 90 }}
                        size="small"
                      />
                      <TextField
                        label="Weight (kg)"
                        type="number"
                        {...register(`exercises.${i}.actualWeightKg`, { valueAsNumber: true })}
                        error={!!rowErrors?.actualWeightKg}
                        helperText={rowErrors?.actualWeightKg?.message}
                        inputProps={{ min: 0, step: 0.5 }}
                        sx={{ flex: 1, minWidth: 90 }}
                        size="small"
                      />
                    </Box>
                    <TextField
                      label="Notes (optional)"
                      {...register(`exercises.${i}.notes`)}
                      fullWidth
                      size="small"
                      sx={{ mt: 1 }}
                      multiline
                      rows={1}
                    />
                  </Box>
                );
              })}
            </Box>
          ) : (
            /* Read-only exercise list */
            <List disablePadding>
              {session.exercises.map((ex, i) => (
                <ListItem
                  key={ex.id}
                  divider={i < session.exercises.length - 1}
                  sx={{ px: 0 }}
                >
                  <ListItemText
                    primary={ex.exerciseName}
                    secondary={
                      <Box component="span" sx={{ display: "block" }}>
                        <Box
                          component="span"
                          sx={{ display: "flex", gap: 0.5, flexWrap: "wrap", mt: 0.5 }}
                        >
                          <Chip
                            component="span"
                            label={`Plan: ${ex.plannedSets}×${ex.plannedReps}${ex.plannedWeightKg != null ? ` @ ${ex.plannedWeightKg}kg` : ""}`}
                            size="small"
                            variant="outlined"
                          />
                          {ex.actualSets != null && (
                            <Chip
                              component="span"
                              label={`Actual: ${ex.actualSets}×${ex.actualReps}${ex.actualWeightKg != null ? ` @ ${ex.actualWeightKg}kg` : ""}`}
                              size="small"
                              color="success"
                              variant="outlined"
                            />
                          )}
                        </Box>
                        {ex.notes && (
                          <Typography
                            component="span"
                            variant="caption"
                            color="text.secondary"
                            sx={{ mt: 0.5, display: "block" }}
                          >
                            {ex.notes}
                          </Typography>
                        )}
                      </Box>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Complete Workout button repeated at the bottom for long exercise lists */}
      {isInProgress && session.exercises.length > 2 && (
        <Box sx={{ mt: 3 }}>
          <Button
            type="submit"
            variant="contained"
            color="success"
            fullWidth
            startIcon={isSubmitting ? undefined : <CheckIcon />}
            disabled={isSubmitting}
          >
            {isSubmitting ? <CircularProgress size={20} color="inherit" /> : "Complete Workout"}
          </Button>
        </Box>
      )}
    </Box>
  );
}
