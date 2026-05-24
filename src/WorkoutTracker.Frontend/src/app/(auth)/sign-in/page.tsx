import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Link from "next/link";
import MuiLink from "@mui/material/Link";
import Typography from "@mui/material/Typography";
import { SignInForm } from "@/components/auth/SignInForm";

export default function SignInPage() {
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
          <SignInForm />
          <Typography variant="body2" textAlign="center" sx={{ mt: 2 }}>
            Don&apos;t have an account?{" "}
            <MuiLink component={Link} href="/sign-up">
              Sign up
            </MuiLink>
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
