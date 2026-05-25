"use client";

import { useRouter } from "next/navigation";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import Chip from "@mui/material/Chip";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import type { WorkoutHistoryResponse } from "@/types/report";

interface HistoryListProps {
  data: WorkoutHistoryResponse;
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    weekday: "short",
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function HistoryList({ data }: HistoryListProps) {
  const router = useRouter();

  if (data.items.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ py: 2 }}>
        No completed workouts for this period
      </Typography>
    );
  }

  return (
    <List disablePadding>
      {data.items.map((item, i) => (
        <ListItem
          key={item.id}
          disablePadding
          divider={i < data.items.length - 1}
        >
          <ListItemButton onClick={() => router.push(`/sessions/${item.id}`)}>
            <ListItemText
              primary={item.title}
              secondary={formatDate(item.completedAtUtc)}
            />
            <Box sx={{ display: "flex", gap: 0.5 }}>
              <Chip
                label={`${item.exerciseCount} ex`}
                size="small"
                variant="outlined"
              />
              <Chip
                label={`${item.totalVolumeKg.toLocaleString()} kg`}
                size="small"
                color="primary"
                variant="outlined"
              />
            </Box>
          </ListItemButton>
        </ListItem>
      ))}
    </List>
  );
}
