import { useCallback, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { ApiException } from "../../../api/worxforgeClient";
import { ROUTES } from "../../../constants/routes.constants";
import { useAuthContext } from "../../../contexts/AuthContext";
import { clearAccessToken, setAccessToken } from "../../../services/tokenService";
import { createAuthenticationService } from "../services/authenticationService";

function toAuthErrorMessage(error: unknown): string {
  if (ApiException.isApiException(error)) {
    if (error.status === 401 || error.status === 403) {
      return "We couldn't sign you in. Please try again.";
    }

    if (error.status >= 500) {
      return "WorxForge is temporarily unavailable. Please try again shortly.";
    }
  }

  if (error instanceof TypeError) {
    return "Unable to connect to WorxForge. Please check your connection and try again.";
  }

  return "We couldn't sign you in. Please try again.";
}

type NavigationState = {
  returnUrl?: string;
};

const authenticationService = createAuthenticationService();

export function useGoogleAuthentication() {
  const navigate = useNavigate();
  const location = useLocation();
  const { setAuthenticatedUser } = useAuthContext();
  const [isAuthenticating, setIsAuthenticating] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const returnUrl = useMemo(() => {
    const state = location.state as NavigationState | null;
    return state?.returnUrl;
  }, [location.state]);

  const authenticateWithGoogleToken = useCallback(async (idToken: string) => {
    const trimmedIdToken = idToken.trim();
    if (!trimmedIdToken || isAuthenticating) {
      return;
    }

    setIsAuthenticating(true);
    setErrorMessage(null);

    try {
      const authToken = await authenticationService.loginWithGoogle(trimmedIdToken);

      if (!authToken.accessToken?.trim()) {
        throw new Error("Missing access token.");
      }

      setAccessToken(authToken.accessToken);

      const user = await authenticationService.getCurrentUser();
      setAuthenticatedUser({
        userId: user.userId,
        displayName: user.displayName,
        avatarUrl: user.avatarUrl,
        role: user.role
      });

      navigate(returnUrl ?? ROUTES.HOME, { replace: true });
    } catch (error) {
      clearAccessToken();
      const message = toAuthErrorMessage(error);
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsAuthenticating(false);
    }
  }, [isAuthenticating, navigate, returnUrl, setAuthenticatedUser]);

  return {
    authenticateWithGoogleToken,
    isAuthenticating,
    errorMessage
  };
}
