import { render, screen } from "@testing-library/react";
import { createMemoryRouter, MemoryRouter, RouterProvider } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { ROUTES } from "../constants/routes.constants";
import { ErrorPage } from "./ErrorPage";

function ThrowingRoute(): never {
  throw new Error("boom");
}

describe("ErrorPage", () => {
  it("renders a 500 message with retry and home actions", () => {
    render(
      <MemoryRouter>
        <ErrorPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("heading", { name: "Something went wrong" })).not.toBeNull();
    expect(screen.getByRole("button", { name: "Try again" })).not.toBeNull();
    expect(screen.getByRole("link", { name: "Back to home" }).getAttribute("href")).toBe(ROUTES.HOME);
  });

  it("renders when used as a router errorElement", () => {
    const consoleError = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const router = createMemoryRouter(
      [
        {
          path: "/",
          errorElement: <ErrorPage />,
          children: [{ index: true, element: <ThrowingRoute /> }]
        }
      ],
      { initialEntries: ["/"] }
    );

    render(<RouterProvider router={router} />);

    expect(screen.getByRole("heading", { name: "Something went wrong" })).not.toBeNull();
    consoleError.mockRestore();
  });
});
