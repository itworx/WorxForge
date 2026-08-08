import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { ROUTES } from "../constants/routes.constants";
import { MainLayout } from "./MainLayout";

function renderLayout(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/" element={<MainLayout />}>
          <Route path={ROUTES.HOME.slice(1)} element={<div>Home page</div>} />
          <Route path={ROUTES.EXPLORE.slice(1)} element={<div>Explore page</div>} />
          <Route path={ROUTES.MY_LIBRARY.slice(1)} element={<div>Library page</div>} />
          <Route path={ROUTES.MY_ASSETS.slice(1)} element={<div>My assets page</div>} />
          <Route path={ROUTES.MY_ASSETS_CREATE.slice(1)} element={<div>Create asset page</div>} />
          <Route path={ROUTES.NOTIFICATIONS.slice(1)} element={<div>Notifications page</div>} />
          <Route path={ROUTES.PROFILE_ME.slice(1)} element={<div>Profile page</div>} />
          <Route path={ROUTES.SETTINGS.slice(1)} element={<div>Settings page</div>} />
        </Route>
      </Routes>
    </MemoryRouter>
  );
}

describe("MainLayout", () => {
  it("cycles desktop sidebar states with one unified toggle", async () => {
    const user = userEvent.setup();

    renderLayout(ROUTES.HOME);

    const desktopToggle = screen.getByRole("button", { name: "Open sidebar" });

    await user.click(desktopToggle);
    expect(screen.getByRole("button", { name: "Collapse sidebar" })).not.toBeNull();

    await user.click(screen.getByRole("button", { name: "Collapse sidebar" }));
    expect(screen.getByRole("button", { name: "Close sidebar" })).not.toBeNull();

    await user.click(screen.getByRole("button", { name: "Close sidebar" }));
    expect(screen.getByRole("button", { name: "Open sidebar" })).not.toBeNull();
  });

  it("activates Publish Asset without also activating My Assets on the create route", async () => {
    const user = userEvent.setup();

    renderLayout(ROUTES.MY_ASSETS_CREATE);

    await user.click(screen.getByRole("button", { name: "Open sidebar" }));

    const publishAssetLink = screen.getByRole("link", { name: "Publish Asset" });
    const myAssetsLink = screen.getByRole("link", { name: "My Assets" });

    expect(publishAssetLink.getAttribute("aria-current")).toBe("page");
    expect(myAssetsLink.getAttribute("aria-current")).toBeNull();
  });

  it("keeps navigation links accessible in collapsed desktop state", async () => {
    const user = userEvent.setup();

    renderLayout(ROUTES.HOME);

    await user.click(screen.getByRole("button", { name: "Open sidebar" }));
    await user.click(screen.getByRole("button", { name: "Collapse sidebar" }));

    expect(screen.getByRole("button", { name: "Close sidebar" })).not.toBeNull();
    expect(screen.getByRole("link", { name: "Home" }).getAttribute("aria-current")).toBe("page");
  });

  it("opens the mobile drawer, focuses the close control, and closes on navigation", async () => {
    const user = userEvent.setup();

    renderLayout(ROUTES.HOME);

    await user.click(screen.getByRole("button", { name: "Open navigation menu" }));

    const dialog = screen.getByRole("dialog", { name: "Primary navigation" });
    const closeButton = within(dialog).getByRole("button", { name: "Close navigation menu" });

    expect(document.activeElement).toBe(closeButton);

    await user.click(within(dialog).getByRole("link", { name: "Explore" }));

    expect(screen.queryByRole("dialog", { name: "Primary navigation" })).toBeNull();
    expect(screen.getByText("Explore page")).not.toBeNull();
  });
});