import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Link from "next/link";
import MuiLink from "@mui/material/Link";
import Typography from "@mui/material/Typography";
import { SignUpForm } from "@/components/auth/SignUpForm";

export default function SignUpPage() {
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
        <CardContent sx={{ p: 4 }}>
          <SignUpForm />
          <Typography variant="body2" textAlign="center" sx={{ mt: 2 }}>
            Already have an account?{" "}
            <MuiLink component={Link} href="/sign-in">
              Sign in
            </MuiLink>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
