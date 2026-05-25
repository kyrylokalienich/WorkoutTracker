"use client";

import { useState } from "react";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import { PageHeader } from "@/components/ui/PageHeader";
import { ProgressStats } from "@/components/reports/ProgressStats";
import { MuscleGroupTable } from "@/components/reports/MuscleGroupTable";
import { HistoryList } from "@/components/reports/HistoryList";
import { useReports } from "@/hooks/useReports";

export default function ReportsPage() {
  const { history, progress, muscleGroups, loading, error, dateRange, setDateRange } =
    useReports();

  const [localFrom, setLocalFrom] = useState(dateRange.from ?? "");
  const [localTo, setLocalTo] = useState(dateRange.to ?? "");

  const handleApply = () => {
    setDateRange({
      from: localFrom || dateRange.from,
      to: localTo || dateRange.to,
    });
  };

  return (
    <Box className="px-4 py-6 max-w-5xl mx-auto w-full">
      <PageHeader title="Reports" />

      {/* Date range picker */}
      <Box sx={{ display: "flex", gap: 2, mb: 4, flexWrap: "wrap", alignItems: "flex-end" }}>
        <TextField
          label="From"
          type="date"
          value={localFrom}
          onChange={(e) => setLocalFrom(e.target.value)}
          InputLabelProps={{ shrink: true }}
          size="small"
        />
        <TextField
          label="To"
          type="date"
          value={localTo}
          onChange={(e) => setLocalTo(e.target.value)}
          InputLabelProps={{ shrink: true }}
          size="small"
        />
        <Button variant="outlined" onClick={handleApply} size="small">
          Apply
        </Button>
      </Box>

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
        <>
          {progress && (
            <Box sx={{ mb: 4 }}>
              <Typography variant="h6" gutterBottom>
                Summary
              </Typography>
              <ProgressStats data={progress} />
            </Box>
          )}

          {muscleGroups && (
            <Card sx={{ mb: 4 }}>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Volume by muscle group
                </Typography>
                <MuscleGroupTable data={muscleGroups} />
              </CardContent>
            </Card>
          )}

          {history && (
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Workout history
                </Typography>
                <HistoryList data={history} />
              </CardContent>
            </Card>
          )}
        </>
      )}
    </Box>
  );
}
