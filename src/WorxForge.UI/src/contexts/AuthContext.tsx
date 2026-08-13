import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";
import { createAuthenticationService } from "../features/authentication/services/authenticationService";
import { clearAccessToken, getAccessToken } from "../services/tokenService";
import type { UserRole } from "../types/app.types";

export interface AuthenticatedUser {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  role: UserRole;
}

type AuthContextValue = {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: AuthenticatedUser | null;
  setAuthenticatedUser: (nextUser: AuthenticatedUser) => void;
  signOut: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue>({
  isAuthenticated: false,
  isLoading: true,
  user: null,
  setAuthenticatedUser: () => undefined,
  signOut: async () => undefined
});

const authenticationService = createAuthenticationService();

function normalizeRole(role: UserRole | string | number): UserRole {
  if (role === "Admin" || role === 1 || role === 2) {
    return "Admin";
  }

  return "User";
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [isLoading, setIsLoading] = useState(true);
  const [user, setUser] = useState<AuthenticatedUser | null>(null);

  const setAuthenticatedUser = useCallback((nextUser: AuthenticatedUser) => {
    setUser({
      ...nextUser,
      role: normalizeRole(nextUser.role)
    });
  }, []);

  const signOut = useCallback(async () => {
    try {
      await authenticationService.logout();
    } catch {
      // Best effort sign out; local token is cleared either way.
    } finally {
      clearAccessToken();
      setUser(null);
    }
  }, []);

  useEffect(() => {
    const restoreSession = async () => {
      const token = getAccessToken();
      if (!token) {
        setIsLoading(false);
        return;
      }

      try {
        const session = await authenticationService.getCurrentUser();
        setUser({
          userId: session.userId,
          displayName: session.displayName,
          avatarUrl: session.avatarUrl,
          role: normalizeRole(session.role)
        });
      } catch {
        clearAccessToken();
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    void restoreSession();
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    isAuthenticated: user !== null,
    isLoading,
    user,
    setAuthenticatedUser,
    signOut
  }), [isLoading, signOut, user, setAuthenticatedUser]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuthContext(): AuthContextValue {
  return useContext(AuthContext);
}

