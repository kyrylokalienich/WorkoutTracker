import { apiRequest } from "./client";
import type {
  SignInRequest,
  SignUpRequest,
  RefreshRequest,
  LogoutRequest,
  AuthResponse,
  RefreshResponse,
  SignUpResponse,
} from "@/types/auth";

export function signIn(req: SignInRequest): Promise<AuthResponse> {
  return apiRequest<AuthResponse>("/api/auth/sign-in", {
    method: "POST",
    body: JSON.stringify(req),
  }, true);
}

export function signUp(req: SignUpRequest): Promise<SignUpResponse> {
  return apiRequest<SignUpResponse>("/api/auth/sign-up", {
    method: "POST",
    body: JSON.stringify(req),
  }, true);
}

export function refresh(req: RefreshRequest): Promise<RefreshResponse> {
  return apiRequest<RefreshResponse>("/api/auth/refresh", {
    method: "POST",
    body: JSON.stringify(req),
  }, true);
}

export function logout(req: LogoutRequest): Promise<{ message: string }> {
  return apiRequest<{ message: string }>("/api/auth/logout", {
    method: "POST",
    body: JSON.stringify(req),
  });
}
