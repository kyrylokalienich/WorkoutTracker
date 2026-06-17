import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Static HTML export — emits a fully static site into `out/` for S3 + CloudFront.
  // No Node server at runtime, so rewrites() are gone; the client calls the API
  // directly via NEXT_PUBLIC_API_URL (see src/lib/api/client.ts) with CORS on the backend.
  output: "export",
  // The export target has no Next image optimization server.
  images: { unoptimized: true },
};

export default nextConfig;
