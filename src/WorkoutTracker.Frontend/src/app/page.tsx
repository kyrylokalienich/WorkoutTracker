"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import CircularProgress from "@mui/material/CircularProgress";
import Box from "@mui/material/Box";

export default function RootPage() {
  const { user, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading) {
      router.replace(user ? "/dashboard" : "/sign-in");
    }
  }, [user, isLoading, router]);

  return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh" }}>
      <CircularProgress />
    </Box>
  );
}
