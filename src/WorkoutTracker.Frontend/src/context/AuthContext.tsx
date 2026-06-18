"use client";

import { useEffect } from "react";
import {
  AuthProvider as OidcProvider,
  useAuth as useOidc,
} from "react-oidc-context";
import { WebStorageStateStore } from "oidc-client-ts";
import { tokenStore } from "@/lib/tokenStore";
import { cognitoConfig } from "@/lib/oidc";

// react-oidc-context configuration for the Cognito Authorization Code flow.
const oidcConfig = {
  authority: cognitoConfig.authority,
  client_id: cognitoConfig.clientId,
  redirect_uri:
    typeof window !== "undefined" ? `${window.location.origin}/auth/callback` : "",
  response_type: "code",
  scope: cognitoConfig.scope,
  // Persist the session across reloads.
  userStore:
    typeof window !== "undefined"
      ? new WebStorageStateStore({ store: window.localStorage })
      : undefined,
  // Strip ?code&state from the URL once the code has been exchanged.
  onSigninCallback: () => {
    window.history.replaceState({}, document.title, window.location.pathname);
  },
};

// Keeps the API client's bearer token in sync with the current OIDC id_token.
function TokenSync() {
  const oidc = useOidc();
  useEffect(() => {
    if (oidc.user?.id_token) tokenStore.set(oidc.user.id_token);
    else tokenStore.clear();
  }, [oidc.user]);
  return null;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  return (
    <OidcProvider {...oidcConfig}>
      <TokenSync />
      {children}
    </OidcProvider>
  );
}

// Backwards-compatible shape so existing components keep working unchanged.
export function useAuth() {
  const oidc = useOidc();
  const profile = oidc.user?.profile;

  const user =
    oidc.isAuthenticated && profile
      ? {
          email: (profile.email as string) ?? "",
          // Friendly display name: the user's name if set, else the local part of
          // their email (the UUID-like cognito:username is intentionally avoided).
          username:
            (profile.name as string) ||
            (profile.email as string)?.split("@")[0] ||
            "",
        }
      : null;

  return {
    user,
    isLoading: oidc.isLoading,
    // Redirect to the Cognito Hosted UI (login + sign-up live there).
    signIn: () => {
      void oidc.signinRedirect();
    },
    // Clear the local session, then hit Cognito's (non-standard) logout endpoint.
    signOut: async () => {
      await oidc.removeUser();
      const logoutUri = `${window.location.origin}/`;
      window.location.href =
        `${cognitoConfig.domain}/logout?client_id=${cognitoConfig.clientId}` +
        `&logout_uri=${encodeURIComponent(logoutUri)}`;
    },
  };
}
