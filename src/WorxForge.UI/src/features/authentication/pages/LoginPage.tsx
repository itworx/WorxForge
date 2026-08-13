import { Navigate, useLocation } from "react-router-dom";
import { APP_NAME } from "../../../constants/app.constants";
import { ROUTES } from "../../../constants/routes.constants";
import { useAuthContext } from "../../../contexts/AuthContext";
import { Logo } from "../../../layouts/components/Logo";
import { GoogleSignInButton } from "../components/GoogleSignInButton";
import { useGoogleAuthentication } from "../hooks/useGoogleAuthentication";

const entranceSteps = {
  logo: "0ms",
  heading: "100ms",
  description: "180ms",
  button: "260ms",
  support: "340ms"
} as const;

export function LoginPage() {
  const { isAuthenticated, isLoading } = useAuthContext();
  const { authenticateWithGoogleToken, errorMessage, isAuthenticating } = useGoogleAuthentication();
  const location = useLocation();

  const returnUrl = (location.state as { returnUrl?: string } | null)?.returnUrl;

  if (!isLoading && isAuthenticated) {
    return <Navigate to={returnUrl ?? ROUTES.HOME} replace />;
  }

  return (
    <section className="w-full rounded-2xl border border-border/80 bg-card/95 p-6 shadow-xl backdrop-blur-sm sm:p-8">
      <div className="space-y-6">
        <div className="wf-section-enter flex items-center justify-center" style={{ animationDelay: entranceSteps.logo }}>
          <Logo className="size-12" />
        </div>

        <div className="space-y-2 text-center">
          <h1 className="wf-section-enter text-2xl font-semibold text-foreground" style={{ animationDelay: entranceSteps.heading }}>
            Welcome to {APP_NAME}
          </h1>
          <p className="wf-section-enter text-sm text-muted-foreground" style={{ animationDelay: entranceSteps.description }}>
            Sign in or create your account with Google.
          </p>
        </div>

        <div className="wf-section-enter" style={{ animationDelay: entranceSteps.button }}>
          <GoogleSignInButton
            disabled={isAuthenticating}
            onCredentialReceived={async (credential) => {
              await authenticateWithGoogleToken(credential);
            }}
          />
        </div>

        <div className="wf-section-enter space-y-2 text-center" style={{ animationDelay: entranceSteps.support }}>
          {isAuthenticating && (
            <p className="wf-loading-pulse text-sm text-muted-foreground" role="status" aria-live="polite">
              Signing you in...
            </p>
          )}
          {errorMessage && (
            <p className="text-sm text-destructive" role="alert" aria-live="assertive">
              {errorMessage}
            </p>
          )}
          <p className="text-xs text-muted-foreground">Secure authentication with Google</p>
        </div>
      </div>
    </section>
  );
}


