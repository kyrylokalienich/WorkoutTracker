"use client";

import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import DeleteIcon from "@mui/icons-material/Delete";
import EventNoteIcon from "@mui/icons-material/EventNote";
import TimerIcon from "@mui/icons-material/Timer";
import PlayArrowIcon from "@mui/icons-material/PlayArrow";
import CheckIcon from "@mui/icons-material/Check";
import { SessionStatusChip } from "./SessionStatusChip";
import { formatDuration, formatShortTime } from "@/lib/utils/time";
import { WorkoutStatus } from "@/types/session";
import type { WorkoutSessionSummaryResponse } from "@/types/session";

interface SessionCardProps {
  session: WorkoutSessionSummaryResponse;
  onClick: () => void;
  onDelete: () => void;
  /** Called when user clicks Start on a Planned session. */
  onStart?: () => void;
  /** Called when user clicks Finish on an InProgress session. */
  onFinish?: () => void;
  /** True while this card's action (start/finish) is in flight. */
  actionLoading?: boolean;
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    weekday: "short",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function SessionCard({
  session,
  onClick,
  onDelete,
  onStart,
  onFinish,
  actionLoading = false,
}: SessionCardProps) {
  const isInProgress = session.status === WorkoutStatus.InProgress;
  const isCompleted = session.status === WorkoutStatus.Completed;

  return (
    <Card
      sx={{
        cursor: "pointer",
        "&:hover": { boxShadow: 4 },
        transition: "box-shadow 0.2s",
        display: "flex",
        flexDirection: "column",
      }}
    >
      <CardContent sx={{ flex: 1, pb: 0 }} onClick={onClick}>
        <Box sx={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", mb: 1 }}>
          <Typography variant="h6" noWrap sx={{ maxWidth: "65%" }}>
            {session.title}
          </Typography>
          <SessionStatusChip status={session.status} />
        </Box>

        <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, color: "text.secondary", mb: 0.5 }}>
          <EventNoteIcon fontSize="small" />
          <Typography variant="caption">
            {formatDate(session.scheduledAtUtc)}
          </Typography>
        </Box>

        {/* Started row */}
        {session.startedAtUtc && (
          <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, color: "text.secondary", mb: 0.5 }}>
            <PlayArrowIcon fontSize="small" />
            <Typography variant="caption">
              Started {formatShortTime(session.startedAtUtc)}
            </Typography>
          </Box>
        )}

        {/* Duration row — static for Completed, formatted for InProgress */}
        {(isInProgress || isCompleted) && session.startedAtUtc && (
          <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, color: isInProgress ? "warning.main" : "success.main", mb: 0.5 }}>
            <TimerIcon fontSize="small" />
            <Typography variant="caption" fontWeight={500}>
              {isCompleted
                ? formatDuration(session.startedAtUtc, session.completedAtUtc)
                : `${formatDuration(session.startedAtUtc)} elapsed`}
            </Typography>
          </Box>
        )}

        <Typography variant="caption" color="text.disabled">
          {session.exerciseCount} exercise{session.exerciseCount !== 1 ? "s" : ""}
        </Typography>
      </CardContent>

      <CardActions sx={{ justifyContent: "space-between", pt: 0 }}>
        {/* Primary action — only one is ever visible at a time */}
        <Box>
          {onStart && (
            <Button
              size="small"
              variant="contained"
              disabled={actionLoading}
              startIcon={
                actionLoading
                  ? <CircularProgress size={14} color="inherit" />
                  : <PlayArrowIcon />
              }
              onClick={(e) => { e.stopPropagation(); onStart(); }}
            >
              Start
            </Button>
          )}
          {onFinish && (
            <Button
              size="small"
              variant="contained"
              color="success"
              disabled={actionLoading}
              startIcon={
                actionLoading
                  ? <CircularProgress size={14} color="inherit" />
                  : <CheckIcon />
              }
              onClick={(e) => { e.stopPropagation(); onFinish(); }}
            >
              Finish
            </Button>
          )}
        </Box>

        <Tooltip title="Delete session">
          <IconButton
            size="small"
            color="error"
            onClick={(e) => {
              e.stopPropagation();
              onDelete();
            }}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </CardActions>
    </Card>
  );
}
