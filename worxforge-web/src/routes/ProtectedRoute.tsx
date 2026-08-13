import type { PropsWithChildren } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { ROUTES } from "../constants/routes.constants";
import { useAuthContext } from "../contexts/AuthContext";

export function ProtectedRoute({ children }: PropsWithChildren) {
  const { isAuthenticated, isLoading } = useAuthContext();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="wf-loading-pulse flex min-h-[40vh] items-center justify-center text-sm text-muted-foreground" role="status" aria-live="polite">
        Restoring your session...
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate
        to={ROUTES.AUTH.LOGIN}
        state={{ returnUrl: `${location.pathname}${location.search}${location.hash}` }}
        replace
      />
    );
  }

  return <>{children}</>;
}

