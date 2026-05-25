"use client";

import { useEffect } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import CircularProgress from "@mui/material/CircularProgress";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import { useWorkoutPlans } from "@/hooks/useWorkoutPlans";
import type { StartNowSessionRequest } from "@/types/session";

const schema = z.object({
  title: z.string().min(1, "Title is required").max(256, "Max 256 characters"),
  workoutPlanId: z.string().optional(),
});

type FormData = z.infer<typeof schema>;

interface StartNowSessionDialogProps {
  open: boolean;
  onClose: () => void;
  onStart: (req: StartNowSessionRequest) => Promise<void>;
}

export function StartNowSessionDialog({ open, onClose, onStart }: StartNowSessionDialogProps) {
  const { plans } = useWorkoutPlans();

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { title: "", workoutPlanId: "" },
  });

  useEffect(() => {
    if (open) reset({ title: "", workoutPlanId: "" });
  }, [open, reset]);

  const onSubmit = async (data: FormData) => {
    await onStart({
      title: data.title.trim(),
      ...(data.workoutPlanId ? { workoutPlanId: Number(data.workoutPlanId) } : {}),
    });
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Start session now</DialogTitle>
      <Box component="form" onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
          <Typography variant="body2" color="text.secondary">
            The session will be created and started immediately.
          </Typography>

          <TextField
            label="Session title"
            {...register("title")}
            error={!!errors.title}
            helperText={errors.title?.message}
            fullWidth
            autoFocus
          />

          <Controller
            name="workoutPlanId"
            control={control}
            render={({ field }) => (
              <TextField
                select
                label="Workout plan (optional)"
                {...field}
                fullWidth
              >
                <MenuItem value="">— No plan —</MenuItem>
                {plans.map((p) => (
                  <MenuItem key={p.id} value={String(p.id)}>
                    {p.name}
                  </MenuItem>
                ))}
              </TextField>
            )}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" color="success" disabled={isSubmitting}>
            {isSubmitting ? <CircularProgress size={20} color="inherit" /> : "Start now"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
