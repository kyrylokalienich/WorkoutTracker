import Chip from "@mui/material/Chip";
import { WorkoutStatus } from "@/types/session";

const statusConfig: Record<
  WorkoutStatus,
  { label: string; color: "info" | "warning" | "success" | "default" }
> = {
  [WorkoutStatus.Planned]: { label: "Planned", color: "info" },
  [WorkoutStatus.InProgress]: { label: "In Progress", color: "warning" },
  [WorkoutStatus.Completed]: { label: "Completed", color: "success" },
  [WorkoutStatus.Skipped]: { label: "Skipped", color: "default" },
};

interface SessionStatusChipProps {
  status: WorkoutStatus;
  size?: "small" | "medium";
}

export function SessionStatusChip({ status, size = "small" }: SessionStatusChipProps) {
  const cfg = statusConfig[status] ?? { label: status, color: "default" as const };
  return <Chip label={cfg.label} color={cfg.color} size={size} />;
}
