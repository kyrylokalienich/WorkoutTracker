import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";

interface EmptyStateProps {
  icon?: React.ReactNode;
  message: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
}

export function EmptyState({
  icon,
  message,
  description,
  actionLabel,
  onAction,
}: EmptyStateProps) {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        py: 8,
        gap: 2,
        textAlign: "center",
        color: "text.secondary",
      }}
    >
      {icon && <Box sx={{ fontSize: 64, opacity: 0.4 }}>{icon}</Box>}
      <Typography variant="h6" color="text.secondary">
        {message}
      </Typography>
      {description && (
        <Typography variant="body2" color="text.disabled">
          {description}
        </Typography>
      )}
      {actionLabel && onAction && (
        <Button variant="contained" onClick={onAction} sx={{ mt: 1 }}>
          {actionLabel}
        </Button>
      )}
    </Box>
  );
}
