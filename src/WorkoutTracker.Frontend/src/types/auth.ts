export interface SignInRequest {
  email: string;
  password: string;
}

export interface SignUpRequest {
  email: string;
  username: string;
  password: string;
  confirmPassword: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface AuthResponse {
  id: number;
  email: string;
  username: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface SignUpResponse {
  message: string;
}

export interface StoredUser {
  id: number;
  email: string;
  username: string;
}
