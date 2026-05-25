"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";
import Divider from "@mui/material/Divider";
import Chip from "@mui/material/Chip";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import CheckIcon from "@mui/icons-material/Check";
import BlockIcon from "@mui/icons-material/Block";
import { PageHeader } from "@/components/ui/PageHeader";
import { LoadingSkeleton } from "@/components/ui/LoadingSkeleton";
import { SessionStatusChip } from "@/components/sessions/SessionStatusChip";
import { CompleteSessionDialog } from "@/components/sessions/CompleteSessionDialog";
import { useSessionDetail } from "@/hooks/useWorkoutSessions";
import { WorkoutStatus } from "@/types/session";

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

  const [completeOpen, setCompleteOpen] = useState(false);

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
    <Box className="px-4 py-6 max-w-3xl mx-auto w-full">
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

      <Box sx={{ display: "flex", gap: 1, mb: 3, flexWrap: "wrap", alignItems: "center" }}>
        <SessionStatusChip status={session.status} size="medium" />
        <Typography variant="caption" color="text.secondary">
          {formatDate(session.scheduledAtUtc)}
        </Typography>
      </Box>

      {session.comments && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {session.comments}
        </Typography>
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
            variant="contained"
            color="success"
            startIcon={<CheckIcon />}
            onClick={() => setCompleteOpen(true)}
          >
            Complete
          </Button>
        )}
      </Box>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Exercises ({session.exercises.length})
          </Typography>
          <List disablePadding>
            {session.exercises.map((ex, i) => (
              <ListItem key={ex.id} divider={i < session.exercises.length - 1} sx={{ px: 0 }}>
                <ListItemText
                  primary={ex.exerciseName}
                  secondary={
                    <Box component="span" sx={{ display: "block" }}>
                      <Box component="span" sx={{ display: "flex", gap: 0.5, flexWrap: "wrap", mt: 0.5 }}>
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
                        <Typography component="span" variant="caption" color="text.secondary" sx={{ mt: 0.5, display: "block" }}>
                          {ex.notes}
                        </Typography>
                      )}
                    </Box>
                  }
                />
              </ListItem>
            ))}
          </List>
        </CardContent>
      </Card>

      {session.exercises.length > 0 && (
        <CompleteSessionDialog
          open={completeOpen}
          session={session}
          onClose={() => setCompleteOpen(false)}
          onComplete={completeSession}
        />
      )}
    </Box>
  );
}
