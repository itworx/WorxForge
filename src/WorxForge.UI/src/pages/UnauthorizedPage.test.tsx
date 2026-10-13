import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { ROUTES } from "../constants/routes.constants";
import { UnauthorizedPage } from "./UnauthorizedPage";

describe("UnauthorizedPage", () => {
  it("renders a 403 message with a link back to home", () => {
    render(
      <MemoryRouter>
        <UnauthorizedPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("heading", { name: "Access restricted" })).not.toBeNull();
    expect(screen.getByRole("link", { name: "Back to home" }).getAttribute("href")).toBe(ROUTES.HOME);
    expect(screen.getByRole("link", { name: "Explore assets" }).getAttribute("href")).toBe(ROUTES.EXPLORE);
  });
});
