import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { AssetDetailsPage } from "./AssetDetailsPage";
import { useAssetDetails } from "../hooks/useAssetDetails";
import { useBookmark } from "../hooks/useBookmark";
import { useAssetReaction } from "../hooks/useAssetReaction";

vi.mock("../hooks/useAssetDetails", () => ({
  useAssetDetails: vi.fn()
}));

vi.mock("../hooks/useBookmark", () => ({
  useBookmark: vi.fn()
}));

vi.mock("../hooks/useAssetReaction", () => ({
  useAssetReaction: vi.fn()
}));

vi.mock("../../comments/components/AssetCommentsSection", () => ({
  AssetCommentsSection: ({ assetId }: { assetId: string }) => (
    <div data-testid="comments-section">Comments section for {assetId}</div>
  )
}));

const assetFixture = {
  assetId: "asset-1",
  title: "Asset title",
  description: "Asset description",
  assetTypeName: "Prompt",
  domainName: "Engineering",
  tools: ["Copilot"],
  authorDisplayName: "Author",
  createdAt: "2026-08-01T00:00:00.000Z",
  updatedAt: "2026-08-02T00:00:00.000Z",
  commentCount: 3,
  isBookmarked: false,
  likeCount: 4,
  dislikeCount: 1,
  markdownContent: "# Header",
  useCaseName: "Review",
  compatibleTools: ["Copilot"],
  authorId: "user-1",
  viewCount: 10,
  isFollowing: false
};

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/assets/asset-1"]}>
      <Routes>
        <Route path="/assets/:assetId" element={<AssetDetailsPage />} />
      </Routes>
    </MemoryRouter>
  );
}

describe("AssetDetailsPage", () => {
  it("renders existing asset content and shows comments in the Discussion tab", async () => {
    vi.mocked(useAssetDetails).mockReturnValue({
      data: assetFixture,
      isLoading: false,
      isError: false
    } as never);

    vi.mocked(useBookmark).mockReturnValue({
      mutate: vi.fn(),
      isPending: false
    } as never);

    vi.mocked(useAssetReaction).mockReturnValue({
      mutate: vi.fn()
    } as never);

    renderPage();

    expect(screen.getByRole("heading", { name: "Asset title" })).not.toBeNull();
    expect(screen.getByText("Discussion (3)")).not.toBeNull();

    await userEvent.click(screen.getByRole("button", { name: "Discussion (3)" }));

    expect(screen.getByTestId("comments-section").textContent).toContain("asset-1");
  });
});
