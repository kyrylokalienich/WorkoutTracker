import { createTheme } from "@mui/material/styles";

export const theme = createTheme({
  palette: {
    primary: { main: "#6750A4" },
    secondary: { main: "#625B71" },
    error: { main: "#B3261E" },
    background: {
      default: "#FFFBFE",
      paper: "#FFFBFE",
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
    h4: { fontWeight: 400 },
    h5: { fontWeight: 400 },
    h6: { fontWeight: 500 },
    body1: { fontSize: "1rem" },
    caption: { fontSize: "0.75rem" },
  },
  shape: { borderRadius: 12 },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: {
        root: { textTransform: "none", borderRadius: 20 },
      },
    },
    MuiCard: {
      defaultProps: { elevation: 1 },
      styleOverrides: {
        root: { borderRadius: 12 },
      },
    },
    MuiTextField: {
      defaultProps: { variant: "outlined", size: "small" },
    },
    MuiChip: {
      styleOverrides: {
        root: { borderRadius: 8 },
      },
    },
    MuiFab: {
      styleOverrides: {
        root: { textTransform: "none" },
      },
    },
  },
});
