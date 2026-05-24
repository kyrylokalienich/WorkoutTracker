"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { tokenStore } from "@/lib/tokenStore";
import { signIn as signInApi, logout as logoutApi } from "@/lib/api/auth";
import type { SignInRequest, StoredUser } from "@/types/auth";

interface AuthContextType {
  user: StoredUser | null;
  isLoading: boolean;
  signIn: (req: SignInRequest) => Promise<void>;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

const USER_KEY = "user";
const REFRESH_TOKEN_KEY = "refreshToken";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<StoredUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    const storedUser = localStorage.getItem(USER_KEY);

    if (!storedRefreshToken || !storedUser) {
      setIsLoading(false);
      return;
    }

    const parsedUser: StoredUser = JSON.parse(storedUser);

    import("@/lib/api/auth").then(({ refresh }) => {
      refresh({ refreshToken: storedRefreshToken })
        .then((data) => {
          tokenStore.set(data.accessToken);
          localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
          setUser(parsedUser);
        })
        .catch(() => {
          localStorage.removeItem(REFRESH_TOKEN_KEY);
          localStorage.removeItem(USER_KEY);
        })
        .finally(() => setIsLoading(false));
    });
  }, []);

  const signIn = useCallback(async (req: SignInRequest) => {
    const data = await signInApi(req);
    tokenStore.set(data.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
    const storedUser: StoredUser = {
      id: data.id,
      email: data.email,
      username: data.username,
    };
    localStorage.setItem(USER_KEY, JSON.stringify(storedUser));
    setUser(storedUser);
  }, []);

  const signOut = useCallback(async () => {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (refreshToken) {
      try {
        await logoutApi({ refreshToken });
      } catch {
        /* best-effort */
      }
    }
    tokenStore.clear();
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, isLoading, signIn, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
