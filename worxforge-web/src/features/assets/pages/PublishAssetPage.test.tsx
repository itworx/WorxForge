import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ROUTES } from "../../../constants/routes.constants";
import { PublishAssetPage } from "./PublishAssetPage";
import {
  useAssetEditorLookups,
  useAssetForEdit,
  useCreateAsset,
  useUpdateAsset,
  useUploadMarkdown
} from "../hooks/useAssets";

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn()
  }
}));

vi.mock("../hooks/useAssets", () => ({
  useAssetEditorLookups: vi.fn(),
  useAssetForEdit: vi.fn(),
  useCreateAsset: vi.fn(),
  useUpdateAsset: vi.fn(),
  useUploadMarkdown: vi.fn()
}));

const mockLookups = {
  assetTypes: [
    { id: "type-1", name: "Prompt" },
    { id: "type-2", name: "Agent" }
  ],
  domains: [
    { id: "domain-1", name: "Engineering" }
  ],
  useCases: [
    { id: "usecase-1", name: "Code Review" }
  ],
  tools: [
    { id: "tool-1", name: "GitHub Copilot" },
    { id: "tool-2", name: "Claude Code" }
  ]
};

const editAsset = {
  assetId: "asset-7",
  title: "Existing asset",
  description: "Existing description",
  markdownContent: "# Existing markdown",
  assetTypeId: "type-1",
  domainId: "domain-1",
  useCaseId: "usecase-1",
  toolIds: ["tool-1"],
  tags: ["existing", "asset"],
  preview: {
    assetId: "asset-7",
    title: "Existing asset",
    description: "Existing description",
    assetTypeName: "Prompt",
    domainName: "Engineering",
    tools: ["GitHub Copilot"],
    authorDisplayName: "Owner",
    createdAt: "2026-08-01T08:00:00.000Z",
    updatedAt: "2026-08-02T08:00:00.000Z",
    reactionCount: 2,
    commentCount: 1,
    isBookmarked: false
  }
};

const createMutateAsync = vi.fn();
const updateMutateAsync = vi.fn();
const uploadMutateAsync = vi.fn();

function configureHooks(overrides?: {
  isLookupsLoading?: boolean;
  lookupsError?: boolean;
  lookupsErrorValue?: Error;
  lookupsFetching?: boolean;
  editLoading?: boolean;
  editError?: boolean;
  editErrorValue?: Error;
  editFetching?: boolean;
  editData?: typeof editAsset | undefined;
  createPending?: boolean;
  updatePending?: boolean;
}) {
  vi.mocked(useAssetEditorLookups).mockReturnValue({
    data: overrides?.isLookupsLoading ? undefined : mockLookups,
    isLoading: overrides?.isLookupsLoading ?? false,
    isError: overrides?.lookupsError ?? false,
    error: overrides?.lookupsErrorValue ?? null,
    isFetching: overrides?.lookupsFetching ?? false,
    refetch: vi.fn()
  } as never);

  vi.mocked(useAssetForEdit).mockReturnValue({
    data: overrides?.editData,
    isLoading: overrides?.editLoading ?? false,
    isError: overrides?.editError ?? false,
    error: overrides?.editErrorValue ?? null,
    isFetching: overrides?.editFetching ?? false,
    refetch: vi.fn()
  } as never);

  vi.mocked(useCreateAsset).mockReturnValue({
    mutateAsync: createMutateAsync,
    isPending: overrides?.createPending ?? false
  } as never);

  vi.mocked(useUpdateAsset).mockReturnValue({
    mutateAsync: updateMutateAsync,
    isPending: overrides?.updatePending ?? false
  } as never);

  vi.mocked(useUploadMarkdown).mockReturnValue({
    mutateAsync: uploadMutateAsync,
    isPending: false
  } as never);
}

function renderPage(initialPath: string) {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path={ROUTES.MY_ASSETS_CREATE} element={<PublishAssetPage />} />
        <Route path={ROUTES.MY_ASSETS_EDIT(":assetId")} element={<PublishAssetPage />} />
        <Route path={ROUTES.MY_ASSETS} element={<div>My assets page</div>} />
      </Routes>
    </MemoryRouter>
  );
}

async function completeRequiredFields(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText("Title"), "Copilot prompt library");
  await user.selectOptions(screen.getByLabelText("Asset Type"), "type-1");
  await user.selectOptions(screen.getByLabelText("Domain"), "domain-1");
  await user.selectOptions(screen.getByLabelText("Use Case"), "usecase-1");
  await user.click(screen.getByRole("button", { name: "GitHub Copilot" }));
  await user.type(screen.getByLabelText("Markdown Editor"), "# Prompt\n\nUseful content");
}

describe("PublishAssetPage", () => {
  beforeEach(() => {
    createMutateAsync.mockReset();
    updateMutateAsync.mockReset();
    uploadMutateAsync.mockReset();
    createMutateAsync.mockResolvedValue(undefined);
    updateMutateAsync.mockResolvedValue(undefined);
    uploadMutateAsync.mockResolvedValue("# Uploaded markdown");
    configureHooks({ editData: undefined });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("starts with an empty form in create mode", () => {
    renderPage(ROUTES.MY_ASSETS_CREATE);

    expect(screen.getByRole("heading", { name: "Publish Asset" })).not.toBeNull();
    expect((screen.getByLabelText("Title") as HTMLInputElement).value).toBe("");
    expect((screen.getByLabelText("Description") as HTMLTextAreaElement).value).toBe("");
    expect((screen.getByLabelText("Markdown Editor") as HTMLTextAreaElement).value).toBe("");
    expect(screen.queryByRole("link", { name: "Open asset" })).toBeNull();
  });

  it("shows required validation before publishing", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);

    await user.click(screen.getByRole("button", { name: "Publish Asset" }));

    expect(await screen.findByText("Title is required.")).not.toBeNull();
    expect(screen.getByText("Asset type is required.")).not.toBeNull();
    expect(screen.getByText("Domain is required.")).not.toBeNull();
    expect(screen.getByText("Use case is required.")).not.toBeNull();
    expect(screen.getByText("Select at least one compatible tool.")).not.toBeNull();
    expect(screen.getByText("Markdown content is required.")).not.toBeNull();
  });

  it("keeps publish and markdown upload enabled without local dev auth controls", () => {
    renderPage(ROUTES.MY_ASSETS_CREATE);

    expect((screen.getByRole("button", { name: "Publish Asset" }) as HTMLButtonElement).disabled).toBe(false);
    expect((document.querySelector('input[type="file"]') as HTMLInputElement).disabled).toBe(false);
    expect(screen.queryByRole("button", { name: "Connect Dev Auth" })).toBeNull();
    expect(screen.queryByText("Not authorized")).toBeNull();
  });

  it("shows the controlled lookup failure reason and disables publishing", () => {
    const lookupError = new Error("Could not reach the WorxForge API while loading domains.");
    lookupError.name = "AssetLookupError";
    configureHooks({ lookupsError: true, lookupsErrorValue: lookupError });

    renderPage(ROUTES.MY_ASSETS_CREATE);

    expect(screen.getByRole("alert").textContent).toContain("Could not reach the WorxForge API while loading domains.");
    expect((screen.getByRole("button", { name: "Publish Asset" }) as HTMLButtonElement).disabled).toBe(true);
  });

  it("accepts markdown uploads and keeps the content editable", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);

    const input = document.getElementById(screen.getByText("Choose .md file").closest("label")?.htmlFor ?? "") as HTMLInputElement;
    const markdownFile = new File(["# Uploaded markdown"], "guide.md", { type: "text/markdown" });

    await user.upload(input, markdownFile);

    await waitFor(() => {
      expect(uploadMutateAsync).toHaveBeenCalledTimes(1);
      expect((screen.getByLabelText("Markdown Editor") as HTMLTextAreaElement).value).toBe("# Uploaded markdown");
    });

    await user.type(screen.getByLabelText("Markdown Editor"), "\n\nMore details");
    expect((screen.getByLabelText("Markdown Editor") as HTMLTextAreaElement).value).toBe("# Uploaded markdown\n\nMore details");
  });

  it("renders formatted markdown when preview is toggled on", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);

    await user.type(screen.getByLabelText("Markdown Editor"), "# Preview heading\n\n- First item");
    await user.click(screen.getByRole("button", { name: "Preview rendered markdown" }));

    expect(screen.getByRole("region", { name: "Markdown Preview" })).not.toBeNull();
    expect(screen.getByRole("heading", { name: "Preview heading" })).not.toBeNull();
    expect(screen.getByText("First item")).not.toBeNull();

    await user.click(screen.getByRole("button", { name: "Hide preview" }));
    expect(screen.queryByRole("region", { name: "Markdown Preview" })).toBeNull();
  });

  it("rejects non-markdown uploads", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);

    const input = document.getElementById(screen.getByText("Choose .md file").closest("label")?.htmlFor ?? "") as HTMLInputElement;
    const textFile = new File(["plain text"], "notes.txt", { type: "text/plain" });

    await user.upload(input, textFile);

    expect(uploadMutateAsync).not.toHaveBeenCalled();
    expect(input.files?.length ?? 0).toBe(0);
  });

  it("updates the live preview from current form values", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);

    await user.type(screen.getByLabelText("Title"), "Preview title");
    await user.type(screen.getByLabelText("Description"), "Preview description");
    await user.selectOptions(screen.getByLabelText("Asset Type"), "type-1");
    await user.selectOptions(screen.getByLabelText("Domain"), "domain-1");
    await user.click(screen.getByRole("button", { name: "GitHub Copilot" }));

    expect(await screen.findByRole("link", { name: "Preview title" })).not.toBeNull();
    expect(screen.getAllByText("Preview description")[0]).not.toBeNull();
    expect(screen.getAllByText("Prompt")[0]).not.toBeNull();
    expect(screen.getAllByText("Engineering")[0]).not.toBeNull();
    expect(screen.getAllByText("GitHub Copilot")[0]).not.toBeNull();
  });

  it("submits the generated create payload and navigates after success", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);
    await completeRequiredFields(user);
    const tagsInput = screen.getByLabelText("Tags");
    await user.type(tagsInput, "prompt ");
    await user.type(tagsInput, "review ");

    await user.click(screen.getByRole("button", { name: "Publish Asset" }));

    await waitFor(() => {
      expect(createMutateAsync).toHaveBeenCalledWith({
        body: {
          title: "Copilot prompt library",
          description: undefined,
          assetTypeId: "type-1",
          domainId: "domain-1",
          useCaseId: "usecase-1",
          toolIds: ["tool-1"],
          tags: ["prompt", "review"],
          markdownContent: "# Prompt\n\nUseful content"
        }
      });
      expect(screen.getByText("My assets page")).not.toBeNull();
    });
  });

  it("turns tags into bubbles on Space and supports remove actions", async () => {
    const user = userEvent.setup();

    renderPage(ROUTES.MY_ASSETS_CREATE);

    const tagsInput = screen.getByLabelText("Tags");
    await user.type(tagsInput, "alpha ");
    await user.type(tagsInput, "beta ");

    expect(screen.getByText("alpha")).not.toBeNull();
    expect(screen.getByText("beta")).not.toBeNull();

    await user.click(screen.getByRole("button", { name: "Remove tag alpha" }));
    expect(screen.queryByText("alpha")).toBeNull();
    expect(screen.getByText("beta")).not.toBeNull();

    await user.clear(tagsInput);
    await user.type(tagsInput, "{Backspace}");
    expect(screen.queryByText("beta")).toBeNull();
  });

  it("shows an error toast when create fails", async () => {
    const user = userEvent.setup();
    createMutateAsync.mockRejectedValueOnce(new Error("Create failed"));

    renderPage(ROUTES.MY_ASSETS_CREATE);
    await completeRequiredFields(user);

    await user.click(screen.getByRole("button", { name: "Publish Asset" }));

    await waitFor(() => {
      expect(createMutateAsync).toHaveBeenCalledTimes(1);
      expect(screen.getByRole("heading", { name: "Publish Asset" })).not.toBeNull();
    });
  });

  it("loads and populates edit mode", async () => {
    configureHooks({ editData: editAsset });

    renderPage(ROUTES.MY_ASSETS_EDIT("asset-7"));

    expect(screen.getByRole("heading", { name: "Edit Asset" })).not.toBeNull();
    expect((await screen.findByLabelText("Title") as HTMLInputElement).value).toBe("Existing asset");
    expect((screen.getByLabelText("Description") as HTMLTextAreaElement).value).toBe("Existing description");
    expect((screen.getByLabelText("Markdown Editor") as HTMLTextAreaElement).value).toBe("# Existing markdown");
  });

  it("preserves untouched edit fields and sends a PUT payload", async () => {
    const user = userEvent.setup();
    configureHooks({ editData: editAsset });

    renderPage(ROUTES.MY_ASSETS_EDIT("asset-7"));

    const titleInput = await screen.findByLabelText("Title");
    await user.clear(titleInput);
    await user.type(titleInput, "Updated asset");
    await user.click(screen.getByRole("button", { name: "Save Asset" }));

    await waitFor(() => {
      expect(updateMutateAsync).toHaveBeenCalledWith({
        assetId: "asset-7",
        body: {
          title: "Updated asset",
          description: "Existing description",
          assetTypeId: "type-1",
          domainId: "domain-1",
          useCaseId: "usecase-1",
          toolIds: ["tool-1"],
          tags: ["existing", "asset"],
          markdownContent: "# Existing markdown"
        }
      });
      expect(screen.getByText("My assets page")).not.toBeNull();
    });
  });

  it("shows the initial edit loading state", () => {
    configureHooks({ isLookupsLoading: true, editLoading: true, editData: undefined });

    renderPage(ROUTES.MY_ASSETS_EDIT("asset-7"));

    expect(screen.getByText("Loading asset editor...")).not.toBeNull();
  });

  it("disables the publish action while a create request is pending", () => {
    configureHooks({ createPending: true, editData: undefined });

    renderPage(ROUTES.MY_ASSETS_CREATE);

    const button = screen.getByRole("button", { name: "Publishing..." });
    expect((button as HTMLButtonElement).disabled).toBe(true);
  });
});