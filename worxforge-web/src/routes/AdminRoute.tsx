import type { PropsWithChildren } from "react";
import { Navigate } from "react-router-dom";
import { ROUTES } from "../constants/routes.constants";
import { useAuthContext } from "../contexts/AuthContext";

export function AdminRoute({ children }: PropsWithChildren) {
  const { isLoading, isAuthenticated, user } = useAuthContext();

  if (isLoading) {
    return (
      <div className="wf-loading-pulse flex min-h-[40vh] items-center justify-center text-sm text-muted-foreground" role="status" aria-live="polite">
        Verifying access...
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to={ROUTES.AUTH.LOGIN} replace />;
  }

  if (user?.role !== "Admin") {
    return <Navigate to={ROUTES.UNAUTHORIZED} replace />;
  }

  return <>{children}</>;
}

