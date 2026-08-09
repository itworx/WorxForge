import { useEffect, useMemo, useRef, useState } from "react";

type GoogleSignInButtonProps = {
  disabled?: boolean;
  onCredentialReceived: (credential: string) => void | Promise<void>;
};

function getGoogleTheme(): "outline" | "filled_black" {
  return document.documentElement.classList.contains("dark") ? "filled_black" : "outline";
}

export function GoogleSignInButton({ disabled = false, onCredentialReceived }: GoogleSignInButtonProps) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const clientId = useMemo(() => import.meta.env.VITE_GOOGLE_CLIENT_ID?.trim() ?? "", []);

  useEffect(() => {
    if (!clientId) {
      setErrorMessage("Google sign-in is not configured for this environment.");
      return;
    }

    let isDisposed = false;
    let retryHandle: ReturnType<typeof window.setTimeout> | undefined;

    const renderButton = () => {
      const googleApi = window.google?.accounts?.id;
      const buttonContainer = containerRef.current;

      if (!googleApi || !buttonContainer) {
        return false;
      }

      googleApi.initialize({
        client_id: clientId,
        callback: (response) => {
          const credential = response.credential?.trim();
          if (!credential) {
            setErrorMessage("Google sign-in could not be completed. Please try again.");
            return;
          }

          setErrorMessage(null);
          void onCredentialReceived(credential);
        },
        auto_select: false,
        cancel_on_tap_outside: true
      });

      buttonContainer.innerHTML = "";
      const buttonWidth = Math.max(220, Math.min(360, Math.floor(buttonContainer.getBoundingClientRect().width - 8)));
      googleApi.renderButton(buttonContainer, {
        type: "standard",
        theme: getGoogleTheme(),
        size: "large",
        text: "continue_with",
        shape: "pill",
        logo_alignment: "left",
        width: buttonWidth
      });

      return true;
    };

    const tryInitialize = () => {
      if (isDisposed) {
        return;
      }

      if (renderButton()) {
        return;
      }

      retryHandle = window.setTimeout(tryInitialize, 100);
    };

    tryInitialize();

    return () => {
      isDisposed = true;
      if (retryHandle) {
        window.clearTimeout(retryHandle);
      }
      window.google?.accounts?.id.cancel();
    };
  }, [clientId, onCredentialReceived]);

  return (
    <div className="space-y-2">
      <div
        className="wf-section-enter rounded-xl border border-border bg-card p-2 shadow-sm transition-transform duration-200 ease-out hover:-translate-y-0.5 hover:shadow-md"
        aria-label="Continue with Google"
      >
        <div
          ref={containerRef}
          aria-busy={disabled}
          className={disabled ? "pointer-events-none opacity-70" : undefined}
        />
      </div>
      {errorMessage && (
        <p className="text-sm text-destructive" role="status" aria-live="polite">
          {errorMessage}
        </p>
      )}
    </div>
  );
}
