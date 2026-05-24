import Box from "@mui/material/Box";
import Skeleton from "@mui/material/Skeleton";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";

interface LoadingSkeletonProps {
  count?: number;
  variant?: "card" | "list" | "text";
}

export function LoadingSkeleton({ count = 3, variant = "card" }: LoadingSkeletonProps) {
  if (variant === "text") {
    return (
      <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
        {Array.from({ length: count }).map((_, i) => (
          <Skeleton key={i} variant="text" height={24} />
        ))}
      </Box>
    );
  }

  if (variant === "list") {
    return (
      <Box sx={{ display: "flex", flexDirection: "column", gap: 1 }}>
        {Array.from({ length: count }).map((_, i) => (
          <Skeleton key={i} variant="rectangular" height={56} sx={{ borderRadius: 1 }} />
        ))}
      </Box>
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: count }).map((_, i) => (
        <Card key={i}>
          <CardContent>
            <Skeleton variant="text" height={28} width="60%" />
            <Skeleton variant="text" height={20} sx={{ mt: 1 }} />
            <Skeleton variant="text" height={20} width="40%" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
