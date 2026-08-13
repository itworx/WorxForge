import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiException } from "../../../api/worxforgeClient";
import { ROUTES } from "../../../constants/routes.constants";
import { useAuthContext } from "../../../contexts/AuthContext";
import { clearAccessToken, setAccessToken } from "../../../services/tokenService";
import { useGoogleAuthentication } from "./useGoogleAuthentication";

const { loginWithGoogle, getCurrentUser } = vi.hoisted(() => ({
  loginWithGoogle: vi.fn(),
  getCurrentUser: vi.fn()
}));

vi.mock("../../../contexts/AuthContext", () => ({
  useAuthContext: vi.fn()
}));

vi.mock("../../../services/tokenService", () => ({
  setAccessToken: vi.fn(),
  clearAccessToken: vi.fn()
}));

vi.mock("../services/authenticationService", () => ({
  createAuthenticationService: () => ({
    loginWithGoogle,
    getCurrentUser,
    logout: vi.fn()
  })
}));

vi.mock("sonner", () => ({
  toast: {
    error: vi.fn()
  }
}));

function HookHarness() {
  const { authenticateWithGoogleToken, errorMessage } = useGoogleAuthentication();

  return (
    <div>
      <button type="button" onClick={() => void authenticateWithGoogleToken("google-id-token")}>
        Authenticate
      </button>
      {errorMessage && <p>{errorMessage}</p>}
    </div>
  );
}

describe("useGoogleAuthentication", () => {
  const setAuthenticatedUser = vi.fn();

  beforeEach(() => {
    loginWithGoogle.mockReset();
    getCurrentUser.mockReset();
    setAuthenticatedUser.mockReset();

    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      user: null,
      setAuthenticatedUser,
      signOut: vi.fn()
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it("logs in with Google token, hydrates user, and navigates to return url", async () => {
    const user = userEvent.setup();
    loginWithGoogle.mockResolvedValue({ accessToken: "backend-token" });
    getCurrentUser.mockResolvedValue({
      userId: "user-1",
      displayName: "A User",
      role: "User",
      avatarUrl: ""
    });

    render(
      <MemoryRouter initialEntries={[{ pathname: ROUTES.AUTH.LOGIN, state: { returnUrl: ROUTES.EXPLORE } }]}>
        <Routes>
          <Route path={ROUTES.AUTH.LOGIN} element={<HookHarness />} />
          <Route path={ROUTES.EXPLORE} element={<div>Explore page</div>} />
        </Routes>
      </MemoryRouter>
    );

    await user.click(screen.getByRole("button", { name: "Authenticate" }));

    await waitFor(() => {
      expect(setAccessToken).toHaveBeenCalledWith("backend-token");
      expect(setAuthenticatedUser).toHaveBeenCalledWith({
        userId: "user-1",
        displayName: "A User",
        role: "User",
        avatarUrl: ""
      });
      expect(screen.getByText("Explore page")).not.toBeNull();
    });
  });

  it("clears local token and surfaces message on backend auth failure", async () => {
    const user = userEvent.setup();
    loginWithGoogle.mockRejectedValue(new ApiException("Unauthorized", 401, "", {}, null));

    render(
      <MemoryRouter initialEntries={[ROUTES.AUTH.LOGIN]}>
        <Routes>
          <Route path={ROUTES.AUTH.LOGIN} element={<HookHarness />} />
        </Routes>
      </MemoryRouter>
    );

    await user.click(screen.getByRole("button", { name: "Authenticate" }));

    await waitFor(() => {
      expect(clearAccessToken).toHaveBeenCalledTimes(1);
      expect(screen.getByText("We couldn't sign you in. Please try again.")).not.toBeNull();
    });
  });
});
