"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import { useAuth } from "@/context/AuthContext";

export default function SignInPage() {
  const { user, isLoading, signIn } = useAuth();
  const router = useRouter();

  // Already signed in → straight to the app.
  useEffect(() => {
    if (!isLoading && user) router.replace("/dashboard");
  }, [user, isLoading, router]);

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        bgcolor: "background.default",
        px: 2,
      }}
    >
      <Card sx={{ width: "100%", maxWidth: 400 }} elevation={2}>
        <CardContent sx={{ p: 4, textAlign: "center" }}>
          <Typography variant="h5" fontWeight={700} gutterBottom>
            WorkoutTracker
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Sign in or create an account to continue.
          </Typography>

          {isLoading ? (
            <CircularProgress />
          ) : (
            <Button variant="contained" size="large" fullWidth onClick={signIn}>
              Sign in with Cognito
            </Button>
          )}
        </CardContent>
      </Card>
    </Box>
  );
}
