import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { ROUTES } from "../constants/routes.constants";
import { NotFoundPage } from "./NotFoundPage";

describe("NotFoundPage", () => {
  it("renders a 404 message with a link back to home", () => {
    render(
      <MemoryRouter>
        <NotFoundPage />
      </MemoryRouter>
    );

    expect(screen.getByRole("heading", { name: "Page not found" })).not.toBeNull();
    expect(screen.getByRole("link", { name: "Back to home" }).getAttribute("href")).toBe(ROUTES.HOME);
    expect(screen.getByRole("link", { name: "Explore assets" }).getAttribute("href")).toBe(ROUTES.EXPLORE);
  });
});
