import { createContext, useContext } from "react";
import type { PropsWithChildren } from "react";
import type { UserRole } from "../types/app.types";

export interface AuthenticatedUser {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  role: UserRole;
}

type AuthContextValue = {
  isAuthenticated: boolean;
  user: AuthenticatedUser | null;
};

const AuthContext = createContext<AuthContextValue>({ isAuthenticated: false, user: null });

export function AuthProvider({ children }: PropsWithChildren) {
  // TODO: Implementation
  return <AuthContext.Provider value={{ isAuthenticated: false, user: null }}>{children}</AuthContext.Provider>;
}

export function useAuthContext(): AuthContextValue {
  // TODO: Implementation
  return useContext(AuthContext);
}

