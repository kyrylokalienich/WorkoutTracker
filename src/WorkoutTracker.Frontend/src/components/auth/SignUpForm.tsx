"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import { signUp } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";

const schema = z
  .object({
    email: z.string().email("Enter a valid email"),
    username: z.string().min(1, "Username is required").max(50, "Max 50 characters"),
    password: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string().min(1, "Please confirm your password"),
  })
  .refine((d) => d.password === d.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type FormData = z.infer<typeof schema>;

export function SignUpForm() {
  const router = useRouter();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormData) => {
    setServerError(null);
    try {
      await signUp({
        email: data.email.trim(),
        username: data.username.trim(),
        password: data.password,
        confirmPassword: data.confirmPassword,
      });
      router.push("/sign-in");
    } catch (e) {
      if (e instanceof ApiError) {
        if (e.details) {
          for (const [field, messages] of Object.entries(e.details)) {
            const key = field.toLowerCase() as keyof FormData;
            if (["email", "username", "password", "confirmPassword"].includes(key)) {
              setError(key, { message: messages[0] });
            }
          }
        } else {
          setServerError(e.message);
        }
      } else {
        setServerError("Connection error — please try again");
      }
    }
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit(onSubmit)}
      sx={{ display: "flex", flexDirection: "column", gap: 2 }}
    >
      <Typography variant="h5" textAlign="center" gutterBottom>
        Create account
      </Typography>

      {serverError && <Alert severity="error">{serverError}</Alert>}

      <TextField
        label="Email"
        type="email"
        autoComplete="email"
        autoFocus
        {...register("email")}
        error={!!errors.email}
        helperText={errors.email?.message}
        fullWidth
      />

      <TextField
        label="Username"
        autoComplete="username"
        {...register("username")}
        error={!!errors.username}
        helperText={errors.username?.message}
        fullWidth
      />

      <TextField
        label="Password"
        type="password"
        autoComplete="new-password"
        {...register("password")}
        error={!!errors.password}
        helperText={errors.password?.message}
        fullWidth
      />

      <TextField
        label="Confirm password"
        type="password"
        autoComplete="new-password"
        {...register("confirmPassword")}
        error={!!errors.confirmPassword}
        helperText={errors.confirmPassword?.message}
        fullWidth
      />

      <Button
        type="submit"
        variant="contained"
        size="large"
        disabled={isSubmitting}
        fullWidth
      >
        {isSubmitting ? <CircularProgress size={22} color="inherit" /> : "Create account"}
      </Button>
    </Box>
  );
}
