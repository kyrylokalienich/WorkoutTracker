import type { Metadata } from "next";
import "./globals.css";
import { Providers } from "@/components/ui/Providers";

export const metadata: Metadata = {
  title: "WorkoutTracker",
  description: "Track your workouts and progress",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
