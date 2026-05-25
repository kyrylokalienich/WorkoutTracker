"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import CircularProgress from "@mui/material/CircularProgress";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import { PageHeader } from "@/components/ui/PageHeader";
import { ProgressStats } from "@/components/reports/ProgressStats";
import { SessionStatusChip } from "@/components/sessions/SessionStatusChip";
import { listSessions } from "@/lib/api/sessions";
import { getProgressReport } from "@/lib/api/reports";
import { defaultDateRange } from "@/hooks/useReports";
import { useAuth } from "@/context/AuthContext";
import { WorkoutStatus } from "@/types/session";
import type { WorkoutSessionSummaryResponse } from "@/types/session";
import type { ProgressReportResponse } from "@/types/report";

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    weekday: "short",
    month: "short",
    day: "numeric",
  });
}

export default function DashboardPage() {
  const { user } = useAuth();
  const router = useRouter();
  const [upcoming, setUpcoming] = useState<WorkoutSessionSummaryResponse[]>([]);
  const [progress, setProgress] = useState<ProgressReportResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const { from, to } = defaultDateRange();

    Promise.all([
      listSessions({ status: WorkoutStatus.Planned, page: 1, pageSize: 5 }),
      getProgressReport({ from, to }),
    ])
      .then(([sessions, prog]) => {
        setUpcoming(sessions.items);
        setProgress(prog);
      })
      .catch((e) => setError(e instanceof Error ? e.message : "Failed to load dashboard"))
      .finally(() => setLoading(false));
  }, []);

  return (
    <Box className="px-4 py-6 max-w-5xl mx-auto w-full">
      <PageHeader title={`Welcome back${user?.username ? `, ${user.username}` : ""}!`} />

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {loading ? (
        <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
          <CircularProgress />
        </Box>
      ) : (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 4 }}>
          {/* Progress stats (last 30 days) */}
          {progress && (
            <Box>
              <Typography variant="h6" gutterBottom>
                Last 30 days
              </Typography>
              <ProgressStats data={progress} />
            </Box>
          )}

          {/* Upcoming sessions */}
          <Card>
            <CardContent>
              <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 1 }}>
                <Typography variant="h6">Upcoming sessions</Typography>
                <Button size="small" onClick={() => router.push("/sessions")}>
                  View all
                </Button>
              </Box>

              {upcoming.length === 0 ? (
                <Box sx={{ py: 3, textAlign: "center" }}>
                  <Typography color="text.secondary" sx={{ mb: 1 }}>
                    No upcoming sessions
                  </Typography>
                  <Button
                    variant="contained"
                    size="small"
                    onClick={() => router.push("/sessions")}
                  >
                    Schedule one
                  </Button>
                </Box>
              ) : (
                <List disablePadding>
                  {upcoming.map((s, i) => (
                    <ListItem
                      key={s.id}
                      disablePadding
                      divider={i < upcoming.length - 1}
                      secondaryAction={<SessionStatusChip status={s.status} />}
                    >
                      <ListItemButton onClick={() => router.push(`/sessions/${s.id}`)}>
                        <ListItemText
                          primary={s.title}
                          secondary={formatDate(s.scheduledAtUtc)}
                        />
                      </ListItemButton>
                    </ListItem>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Box>
      )}
    </Box>
  );
}
