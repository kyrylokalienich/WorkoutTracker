"use client";

import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import Typography from "@mui/material/Typography";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import Box from "@mui/material/Box";
import DeleteIcon from "@mui/icons-material/Delete";
import EventNoteIcon from "@mui/icons-material/EventNote";
import { SessionStatusChip } from "./SessionStatusChip";
import type { WorkoutSessionSummaryResponse } from "@/types/session";

interface SessionCardProps {
  session: WorkoutSessionSummaryResponse;
  onClick: () => void;
  onDelete: () => void;
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

export function SessionCard({ session, onClick, onDelete }: SessionCardProps) {
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

        <Typography variant="caption" color="text.disabled">
          {session.exerciseCount} exercise{session.exerciseCount !== 1 ? "s" : ""}
        </Typography>
      </CardContent>

      <CardActions sx={{ justifyContent: "flex-end", pt: 0 }}>
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
