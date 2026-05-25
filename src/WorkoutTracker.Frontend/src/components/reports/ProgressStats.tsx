import Grid from "@mui/material/Grid2";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Typography from "@mui/material/Typography";
import type { ProgressReportResponse } from "@/types/report";

interface StatCardProps {
  label: string;
  value: string;
  sub?: string;
}

function StatCard({ label, value, sub }: StatCardProps) {
  return (
    <Card>
      <CardContent>
        <Typography variant="caption" color="text.secondary" display="block">
          {label}
        </Typography>
        <Typography variant="h4" fontWeight={700} color="primary">
          {value}
        </Typography>
        {sub && (
          <Typography variant="caption" color="text.secondary">
            {sub}
          </Typography>
        )}
      </CardContent>
    </Card>
  );
}

interface ProgressStatsProps {
  data: ProgressReportResponse;
}

export function ProgressStats({ data }: ProgressStatsProps) {
  return (
    <Grid container spacing={2}>
      <Grid size={{ xs: 6, sm: 4, md: 2 }}>
        <StatCard
          label="Workouts completed"
          value={String(data.completedWorkoutCount)}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 4, md: 2 }}>
        <StatCard
          label="Total volume"
          value={`${(data.totalVolumeKg / 1000).toFixed(1)}t`}
          sub={`${data.totalVolumeKg.toLocaleString()} kg`}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 4, md: 2 }}>
        <StatCard
          label="Avg volume / workout"
          value={`${Math.round(data.averageVolumeKgPerWorkout)} kg`}
        />
      </Grid>
      <Grid size={{ xs: 6, sm: 4, md: 2 }}>
        <StatCard
          label="Completion rate"
          value={`${Math.round(data.completionRate * 100)}%`}
          sub={`${data.scheduledCompletedCount} of ${data.scheduledCompletedCount + data.scheduledSkippedCount}`}
        />
      </Grid>
    </Grid>
  );
}
