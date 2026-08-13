import { render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { GoogleSignInButton } from "./GoogleSignInButton";

describe("GoogleSignInButton", () => {
  const onCredentialReceived = vi.fn<(credential: string) => void>();
  const initialize = vi.fn();
  const renderButton = vi.fn();
  const cancel = vi.fn();

  beforeEach(() => {
    vi.stubEnv("VITE_GOOGLE_CLIENT_ID", "test-client-id");
    onCredentialReceived.mockReset();
    initialize.mockReset();
    renderButton.mockReset();
    cancel.mockReset();

    Object.defineProperty(window, "google", {
      value: {
        accounts: {
          id: {
            initialize,
            renderButton,
            cancel,
            prompt: vi.fn()
          }
        }
      },
      configurable: true,
      writable: true
    });
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it("renders the official Google button through GIS", async () => {
    render(<GoogleSignInButton onCredentialReceived={onCredentialReceived} />);

    await waitFor(() => {
      expect(initialize).toHaveBeenCalledTimes(1);
      expect(renderButton).toHaveBeenCalledTimes(1);
    });
  });

  it("forwards Google credential to the caller", async () => {
    render(<GoogleSignInButton onCredentialReceived={onCredentialReceived} />);

    await waitFor(() => {
      expect(initialize).toHaveBeenCalledTimes(1);
    });

    const config = initialize.mock.calls[0]?.[0] as { callback: (response: { credential?: string }) => void };
    config.callback({ credential: "google-id-token" });

    expect(onCredentialReceived).toHaveBeenCalledWith("google-id-token");
  });

  it("shows a configuration error when client id is missing", async () => {
    vi.stubEnv("VITE_GOOGLE_CLIENT_ID", "");

    render(<GoogleSignInButton onCredentialReceived={onCredentialReceived} />);

    expect(await screen.findByText("Google sign-in is not configured for this environment.")).not.toBeNull();
    expect(renderButton).not.toHaveBeenCalled();
  });
});
