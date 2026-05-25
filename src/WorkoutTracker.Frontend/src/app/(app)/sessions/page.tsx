"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Fab from "@mui/material/Fab";
import Alert from "@mui/material/Alert";
import Chip from "@mui/material/Chip";
import Pagination from "@mui/material/Pagination";
import AddIcon from "@mui/icons-material/Add";
import EventNoteIcon from "@mui/icons-material/EventNote";
import { PageHeader } from "@/components/ui/PageHeader";
import { EmptyState } from "@/components/ui/EmptyState";
import { LoadingSkeleton } from "@/components/ui/LoadingSkeleton";
import { SessionCard } from "@/components/sessions/SessionCard";
import { ScheduleSessionDialog } from "@/components/sessions/ScheduleSessionDialog";
import { CompleteSessionDialog } from "@/components/sessions/CompleteSessionDialog";
import { useWorkoutSessions } from "@/hooks/useWorkoutSessions";
import { WorkoutStatus } from "@/types/session";

const statusFilters: Array<{ label: string; value: WorkoutStatus | undefined }> = [
  { label: "All", value: undefined },
  { label: "Planned", value: WorkoutStatus.Planned },
  { label: "In Progress", value: WorkoutStatus.InProgress },
  { label: "Completed", value: WorkoutStatus.Completed },
  { label: "Skipped", value: WorkoutStatus.Skipped },
];

export default function SessionsPage() {
  const router = useRouter();
  const {
    sessions,
    totalCount,
    page,
    pageSize,
    loading,
    error,
    statusFilter,
    actionLoading,
    sessionToComplete,
    setPage,
    setStatusFilter,
    scheduleSession,
    deleteSession,
    startSession,
    openFinishDialog,
    finishSession,
    closeFinishDialog,
  } = useWorkoutSessions();

  const [scheduleOpen, setScheduleOpen] = useState(false);

  const totalPages = Math.ceil(totalCount / pageSize);

  if (loading && sessions.length === 0) {
    return (
      <Box className="px-4 py-6 max-w-5xl mx-auto w-full">
        <PageHeader title="Sessions" />
        <LoadingSkeleton count={4} variant="card" />
      </Box>
    );
  }

  return (
    <Box className="px-4 py-6 max-w-5xl mx-auto w-full">
      <PageHeader title="Sessions" />

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Status filter chips */}
      <Box sx={{ display: "flex", gap: 1, mb: 3, flexWrap: "wrap" }}>
        {statusFilters.map((sf) => (
          <Chip
            key={sf.label}
            label={sf.label}
            variant={statusFilter === sf.value ? "filled" : "outlined"}
            color={statusFilter === sf.value ? "primary" : "default"}
            onClick={() => setStatusFilter(sf.value)}
            clickable
          />
        ))}
      </Box>

      {sessions.length === 0 ? (
        <EmptyState
          icon={<EventNoteIcon sx={{ fontSize: 64 }} />}
          message="No sessions found"
          description={
            statusFilter
              ? `No ${statusFilter.toLowerCase()} sessions`
              : "Schedule your first workout session"
          }
          actionLabel={!statusFilter ? "Schedule session" : undefined}
          onAction={!statusFilter ? () => setScheduleOpen(true) : undefined}
        />
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {sessions.map((s) => (
              <SessionCard
                key={s.id}
                session={s}
                onClick={() => router.push(`/sessions/${s.id}`)}
                onDelete={() => deleteSession(s.id)}
                onStart={s.status === WorkoutStatus.Planned ? () => startSession(s.id) : undefined}
                onFinish={s.status === WorkoutStatus.InProgress ? () => openFinishDialog(s.id) : undefined}
                actionLoading={actionLoading === s.id}
              />
            ))}
          </div>

          {totalPages > 1 && (
            <Box sx={{ display: "flex", justifyContent: "center", mt: 4 }}>
              <Pagination
                count={totalPages}
                page={page}
                onChange={(_, p) => setPage(p)}
                color="primary"
              />
            </Box>
          )}
        </>
      )}

      <Fab
        color="primary"
        aria-label="Schedule session"
        onClick={() => setScheduleOpen(true)}
        sx={{ position: "fixed", bottom: { xs: 80, sm: 24 }, right: 24 }}
      >
        <AddIcon />
      </Fab>

      <ScheduleSessionDialog
        open={scheduleOpen}
        onClose={() => setScheduleOpen(false)}
        onSchedule={scheduleSession}
      />

      {sessionToComplete && (
        <CompleteSessionDialog
          open
          session={sessionToComplete}
          onClose={closeFinishDialog}
          onComplete={finishSession}
        />
      )}
    </Box>
  );
}
