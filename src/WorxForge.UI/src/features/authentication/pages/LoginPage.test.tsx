import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { ROUTES } from "../../../constants/routes.constants";
import { useAuthContext } from "../../../contexts/AuthContext";
import { LoginPage } from "./LoginPage";

vi.mock("../../../contexts/AuthContext", () => ({
  useAuthContext: vi.fn()
}));

vi.mock("../hooks/useGoogleAuthentication", () => ({
  useGoogleAuthentication: vi.fn(() => ({
    authenticateWithGoogleToken: vi.fn(),
    errorMessage: null,
    isAuthenticating: false
  }))
}));

vi.mock("../components/GoogleSignInButton", () => ({
  GoogleSignInButton: ({ disabled }: { disabled?: boolean }) => (
    <div aria-label="Continue with Google" data-disabled={disabled ? "true" : "false"}>GoogleButton</div>
  )
}));

describe("LoginPage", () => {
  it("renders WorxForge branding and Google auth copy", () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: false,
      isLoading: false,
      user: null,
      setAuthenticatedUser: vi.fn(),
      signOut: vi.fn()
    });

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("heading", { name: "Welcome to WorxForge" })).not.toBeNull();
    expect(screen.getByText("Sign in or create your account with Google.")).not.toBeNull();
    expect(screen.getByText("Secure authentication with Google")).not.toBeNull();
  });

  it("redirects authenticated users to return url", () => {
    vi.mocked(useAuthContext).mockReturnValue({
      isAuthenticated: true,
      isLoading: false,
      user: {
        userId: "u1",
        displayName: "Admin User",
        role: "Admin"
      },
      setAuthenticatedUser: vi.fn(),
      signOut: vi.fn()
    });

    render(
      <MemoryRouter initialEntries={[{ pathname: ROUTES.AUTH.LOGIN, state: { returnUrl: ROUTES.EXPLORE } }]}>
        <Routes>
          <Route path={ROUTES.AUTH.LOGIN} element={<LoginPage />} />
          <Route path={ROUTES.EXPLORE} element={<div>Explore page</div>} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText("Explore page")).not.toBeNull();
  });
});
