"use client";

import { useEffect } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import dayjs, { Dayjs } from "dayjs";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import CircularProgress from "@mui/material/CircularProgress";
import Box from "@mui/material/Box";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import { useWorkoutPlans } from "@/hooks/useWorkoutPlans";
import type { ScheduleSessionRequest } from "@/types/session";

const schema = z.object({
  title: z.string().min(1, "Title is required").max(256, "Max 256 characters"),
  workoutPlanId: z.string().optional(),
  scheduledAt: z.custom<Dayjs>((v) => dayjs.isDayjs(v) && v.isValid(), {
    message: "Select a valid date and time",
  }),
});

type FormData = z.infer<typeof schema>;

interface ScheduleSessionDialogProps {
  open: boolean;
  onClose: () => void;
  onSchedule: (req: ScheduleSessionRequest) => Promise<void>;
}

export function ScheduleSessionDialog({
  open,
  onClose,
  onSchedule,
}: ScheduleSessionDialogProps) {
  const { plans } = useWorkoutPlans();

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: "",
      workoutPlanId: "",
      scheduledAt: dayjs().add(1, "hour").startOf("hour"),
    },
  });

  useEffect(() => {
    if (open) {
      reset({
        title: "",
        workoutPlanId: "",
        scheduledAt: dayjs().add(1, "hour").startOf("hour"),
      });
    }
  }, [open, reset]);

  const onSubmit = async (data: FormData) => {
    const req: ScheduleSessionRequest = {
      title: data.title.trim(),
      scheduledAtUtc: data.scheduledAt.toISOString(),
      ...(data.workoutPlanId ? { workoutPlanId: Number(data.workoutPlanId) } : {}),
    };
    await onSchedule(req);
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Schedule session</DialogTitle>
      <Box component="form" onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
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
                error={!!errors.workoutPlanId}
                helperText={errors.workoutPlanId?.message}
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

          <Controller
            name="scheduledAt"
            control={control}
            render={({ field }) => (
              <DateTimePicker
                label="Scheduled date & time"
                value={field.value}
                onChange={(val) => field.onChange(val)}
                slotProps={{
                  textField: {
                    fullWidth: true,
                    error: !!errors.scheduledAt,
                    helperText: errors.scheduledAt?.message,
                  },
                }}
              />
            )}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? <CircularProgress size={20} color="inherit" /> : "Schedule"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
