"use client";

import { useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import Box from "@mui/material/Box";
import Drawer from "@mui/material/Drawer";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import BottomNavigation from "@mui/material/BottomNavigation";
import BottomNavigationAction from "@mui/material/BottomNavigationAction";
import Paper from "@mui/material/Paper";
import Tooltip from "@mui/material/Tooltip";
import IconButton from "@mui/material/IconButton";
import Typography from "@mui/material/Typography";
import Divider from "@mui/material/Divider";
import DashboardIcon from "@mui/icons-material/Dashboard";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import EventNoteIcon from "@mui/icons-material/EventNote";
import BarChartIcon from "@mui/icons-material/BarChart";
import LogoutIcon from "@mui/icons-material/Logout";
import { useAuth } from "@/context/AuthContext";

const DRAWER_WIDTH = 220;

const navItems = [
  { label: "Dashboard", href: "/dashboard", icon: <DashboardIcon /> },
  { label: "Plans", href: "/plans", icon: <FitnessCenterIcon /> },
  { label: "Sessions", href: "/sessions", icon: <EventNoteIcon /> },
  { label: "Reports", href: "/reports", icon: <BarChartIcon /> },
];

export function AppShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { signOut } = useAuth();
  const [signingOut, setSigningOut] = useState(false);

  const currentIndex = navItems.findIndex((item) =>
    pathname.startsWith(item.href)
  );

  const handleNavClick = (href: string) => {
    router.push(href);
  };

  const handleSignOut = async () => {
    setSigningOut(true);
    await signOut();
    router.push("/sign-in");
  };

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      {/* Desktop sidebar */}
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: "none", sm: "block" },
          width: DRAWER_WIDTH,
          flexShrink: 0,
          "& .MuiDrawer-paper": {
            width: DRAWER_WIDTH,
            boxSizing: "border-box",
            borderRight: "1px solid",
            borderColor: "divider",
          },
        }}
      >
        <Box sx={{ p: 2, pb: 1 }}>
          <Typography variant="h6" color="primary" fontWeight={700}>
            WorkoutTracker
          </Typography>
        </Box>
        <Divider />
        <List sx={{ flex: 1, pt: 1 }}>
          {navItems.map((item) => {
            const active = pathname.startsWith(item.href);
            return (
              <ListItem key={item.href} disablePadding>
                <ListItemButton
                  selected={active}
                  onClick={() => handleNavClick(item.href)}
                  sx={{ borderRadius: 2, mx: 1, mb: 0.5 }}
                >
                  <ListItemIcon
                    sx={{ minWidth: 36, color: active ? "primary.main" : "inherit" }}
                  >
                    {item.icon}
                  </ListItemIcon>
                  <ListItemText primary={item.label} />
                </ListItemButton>
              </ListItem>
            );
          })}
        </List>
        <Divider />
        <Box sx={{ p: 1 }}>
          <Tooltip title="Sign out">
            <ListItemButton
              onClick={handleSignOut}
              disabled={signingOut}
              sx={{ borderRadius: 2 }}
            >
              <ListItemIcon sx={{ minWidth: 36 }}>
                <LogoutIcon />
              </ListItemIcon>
              <ListItemText primary="Sign out" />
            </ListItemButton>
          </Tooltip>
        </Box>
      </Drawer>

      {/* Main content */}
      <Box
        component="main"
        sx={{
          flex: 1,
          display: "flex",
          flexDirection: "column",
          pb: { xs: 8, sm: 0 },
          overflow: "auto",
        }}
      >
        {children}
      </Box>

      {/* Mobile bottom nav */}
      <Paper
        sx={{
          display: { xs: "block", sm: "none" },
          position: "fixed",
          bottom: 0,
          left: 0,
          right: 0,
          zIndex: 1200,
        }}
        elevation={3}
      >
        <BottomNavigation
          value={currentIndex}
          onChange={(_, value: number) => handleNavClick(navItems[value].href)}
        >
          {navItems.map((item) => (
            <BottomNavigationAction
              key={item.href}
              label={item.label}
              icon={item.icon}
            />
          ))}
        </BottomNavigation>
      </Paper>
    </Box>
  );
}
