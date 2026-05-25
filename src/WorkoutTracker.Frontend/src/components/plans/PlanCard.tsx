"use client";

import { useState } from "react";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import CardActions from "@mui/material/CardActions";
import Typography from "@mui/material/Typography";
import Chip from "@mui/material/Chip";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import Box from "@mui/material/Box";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import { ConfirmDialog } from "@/components/ui/ConfirmDialog";
import type { WorkoutPlanSummaryResponse } from "@/types/plan";

interface PlanCardProps {
  plan: WorkoutPlanSummaryResponse;
  onClick: () => void;
  onEdit: () => void;
  onDelete: () => Promise<void> | void;
}

export function PlanCard({ plan, onClick, onEdit, onDelete }: PlanCardProps) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const handleDeleteConfirm = async () => {
    setDeleteLoading(true);
    try {
      await onDelete();
    } finally {
      setDeleteLoading(false);
      setConfirmOpen(false);
    }
  };

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
          <Typography variant="h6" noWrap sx={{ maxWidth: "70%" }}>
            {plan.name}
          </Typography>
          {!plan.isActive && (
            <Chip label="Inactive" size="small" color="default" />
          )}
        </Box>
        {plan.description && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            {plan.description}
          </Typography>
        )}
        <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, color: "text.secondary" }}>
          <FitnessCenterIcon fontSize="small" />
          <Typography variant="caption">
            {plan.exerciseCount} exercise{plan.exerciseCount !== 1 ? "s" : ""}
          </Typography>
        </Box>
      </CardContent>
      <CardActions sx={{ justifyContent: "flex-end", pt: 0 }}>
        <Tooltip title="Edit plan">
          <IconButton
            size="small"
            onClick={(e) => {
              e.stopPropagation();
              onEdit();
            }}
          >
            <EditIcon fontSize="small" />
          </IconButton>
        </Tooltip>
        <Tooltip title="Delete plan">
          <IconButton
            size="small"
            color="error"
            onClick={(e) => { e.stopPropagation(); setConfirmOpen(true); }}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </CardActions>

      <ConfirmDialog
        open={confirmOpen}
        title="Delete plan"
        message={`Delete "${plan.name}"? This cannot be undone.`}
        loading={deleteLoading}
        onConfirm={handleDeleteConfirm}
        onClose={() => setConfirmOpen(false)}
      />
    </Card>
  );
}
