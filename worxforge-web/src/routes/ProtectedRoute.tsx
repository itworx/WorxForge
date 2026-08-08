import type { PropsWithChildren } from "react";
import { Navigate } from "react-router-dom";

export function ProtectedRoute({ children }: PropsWithChildren) {
  // TODO: Implementation
  const isAuthenticated = true;
  return isAuthenticated ? <>{children}</> : <Navigate to="/auth/login" replace />;
}

