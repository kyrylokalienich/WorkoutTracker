import { tokenStore } from "@/lib/tokenStore";
import type { RefreshResponse } from "@/types/auth";

// Absolute API origin, baked at build time. Empty string keeps relative paths
// (e.g. for local `next dev`); in the static export it points at the public API host.
const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "";

export class ApiError extends Error {
  constructor(
    public readonly code: string,
    message: string,
    public readonly details?: Record<string, string[]>
  ) {
    super(message);
    this.name = "ApiError";
  }
}

let isRefreshing = false;

async function tryRefreshToken(): Promise<boolean> {
  if (isRefreshing) {
    await new Promise((resolve) => setTimeout(resolve, 500));
    return tokenStore.get() !== null;
  }

  const refreshToken =
    typeof window !== "undefined"
      ? localStorage.getItem("refreshToken")
      : null;

  if (!refreshToken) return false;

  isRefreshing = true;
  try {
    const response = await fetch(`${API_BASE}/api/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        ...(tokenStore.get()
          ? { Authorization: `Bearer ${tokenStore.get()}` }
          : {}),
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) return false;

    const data: RefreshResponse = await response.json();
    tokenStore.set(data.accessToken);
    if (typeof window !== "undefined") {
      localStorage.setItem("refreshToken", data.refreshToken);
    }
    return true;
  } catch {
    return false;
  } finally {
    isRefreshing = false;
  }
}

export async function apiRequest<T>(
  path: string,
  options: RequestInit = {},
  skipAuth = false
): Promise<T> {
  const token = skipAuth ? null : tokenStore.get();

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (response.status === 401 && !skipAuth) {
    const refreshed = await tryRefreshToken();
    if (refreshed) {
      return apiRequest<T>(path, options, false);
    }
    tokenStore.clear();
    if (typeof window !== "undefined") {
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("user");
      window.location.href = "/sign-in";
    }
    throw new ApiError("unauthorized", "Session expired");
  }

  if (response.status === 204) {
    return undefined as T;
  }

  if (!response.ok) {
    let errorData: {
      code?: string;
      message?: string;
      details?: Record<string, string[]>;
    } = {};
    try {
      errorData = await response.json();
    } catch {
      /* empty */
    }
    throw new ApiError(
      errorData.code ?? "unknown_error",
      errorData.message ?? "An unexpected error occurred",
      errorData.details
    );
  }

  return response.json() as Promise<T>;
}
