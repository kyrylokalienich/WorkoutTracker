"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import FormControlLabel from "@mui/material/FormControlLabel";
import Switch from "@mui/material/Switch";
import CircularProgress from "@mui/material/CircularProgress";
import Box from "@mui/material/Box";
import type { CreatePlanRequest } from "@/types/plan";

interface PlanLike {
  name: string;
  description?: string | null;
  isActive: boolean;
}

const schema = z.object({
  name: z.string().min(1, "Name is required").max(256, "Max 256 characters"),
  description: z.string().max(1000, "Max 1000 characters").optional(),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

interface CreateEditPlanDialogProps {
  open: boolean;
  plan?: PlanLike | null;
  onClose: () => void;
  onSave: (req: CreatePlanRequest) => Promise<void>;
}

export function CreateEditPlanDialog({
  open,
  plan,
  onClose,
  onSave,
}: CreateEditPlanDialogProps) {
  const isEdit = !!plan;

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", description: "", isActive: true },
  });

  const isActive = watch("isActive");

  useEffect(() => {
    if (open) {
      reset({
        name: plan?.name ?? "",
        description: plan?.description ?? "",
        isActive: plan?.isActive ?? true,
      });
    }
  }, [open, plan, reset]);

  const onSubmit = async (data: FormData) => {
    await onSave({
      name: data.name.trim(),
      description: data.description?.trim() || undefined,
      isActive: data.isActive,
    });
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{isEdit ? "Edit plan" : "Create plan"}</DialogTitle>
      <Box component="form" onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          <TextField
            label="Plan name"
            {...register("name")}
            error={!!errors.name}
            helperText={errors.name?.message}
            fullWidth
            autoFocus
          />
          <TextField
            label="Description (optional)"
            {...register("description")}
            error={!!errors.description}
            helperText={errors.description?.message}
            fullWidth
            multiline
            rows={2}
          />
          <FormControlLabel
            control={
              <Switch
                checked={isActive}
                onChange={(e) => setValue("isActive", e.target.checked)}
              />
            }
            label="Active"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? <CircularProgress size={20} color="inherit" /> : isEdit ? "Save" : "Create"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
