import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { createAssetsService } from "./assetsService";

const lookupPayloads: Record<string, Array<{ id: string; name: string }>> = {
  "/api/assets/types": [{ id: "type-1", name: "Prompt" }],
  "/api/assets/domains": [{ id: "domain-1", name: "Development" }],
  "/api/assets/use-cases": [{ id: "use-case-1", name: "Code review" }],
  "/api/assets/tools": [{ id: "tool-1", name: "GitHub Copilot" }]
};

describe("createAssetsService", () => {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    fetchMock.mockImplementation(async (input) => {
      const url = new URL(input.toString(), "http://localhost");
      const payload = lookupPayloads[url.pathname];

      if (!payload) {
        return new Response(null, { status: 404 });
      }

      return new Response(JSON.stringify(payload), {
        status: 200,
        headers: { "Content-Type": "application/json" }
      });
    });
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("loads editor lookups from the public asset endpoints", async () => {
    const service = createAssetsService("http://localhost:5222/");

    await expect(service.getEditorLookups()).resolves.toEqual({
      assetTypes: lookupPayloads["/api/assets/types"],
      domains: lookupPayloads["/api/assets/domains"],
      useCases: lookupPayloads["/api/assets/use-cases"],
      tools: lookupPayloads["/api/assets/tools"]
    });

    const requestedPaths = fetchMock.mock.calls.map(([input]) => new URL(input.toString(), "http://localhost").pathname);
    expect(requestedPaths).toEqual([
      "/api/assets/types",
      "/api/assets/domains",
      "/api/assets/use-cases",
      "/api/assets/tools"
    ]);
    expect(requestedPaths).not.toContain("/api/explore/filters");
  });

  it("reports which lookup request failed", async () => {
    fetchMock.mockImplementation(async (input) => {
      const url = new URL(input.toString(), "http://localhost");
      return url.pathname === "/api/assets/domains"
        ? new Response(null, { status: 503 })
        : new Response(JSON.stringify(lookupPayloads[url.pathname]), {
            status: 200,
            headers: { "Content-Type": "application/json" }
          });
    });

    const service = createAssetsService("http://localhost:5222");

    await expect(service.getEditorLookups()).rejects.toThrow("The API could not load domains (HTTP 503).");
  });

  it("uses same-origin API paths when the base URL is empty", async () => {
    const service = createAssetsService("");

    await service.getEditorLookups();

    expect(fetchMock.mock.calls.map(([input]) => input.toString())).toEqual([
      "/api/assets/types",
      "/api/assets/domains",
      "/api/assets/use-cases",
      "/api/assets/tools"
    ]);
  });

  it("returns editable Markdown content from the upload response envelope", async () => {
    fetchMock.mockResolvedValueOnce(new Response(JSON.stringify({ markdownContent: "# Uploaded guide" }), {
      status: 200,
      headers: { "Content-Type": "application/json" }
    }));

    const service = createAssetsService("https://localhost:7235");
    const file = new File(["# Uploaded guide"], "guide.md", { type: "text/markdown" });

    await expect(service.uploadMarkdown(file)).resolves.toBe("# Uploaded guide");

    const [, request] = fetchMock.mock.calls[0];
    expect(request?.method).toBe("POST");
    expect(request?.body).toBeInstanceOf(FormData);
  });

  it("treats the backend 201 Created response as a successful publish", async () => {
    fetchMock.mockResolvedValueOnce(new Response(JSON.stringify({ assetId: "asset-1" }), {
      status: 201,
      headers: {
        "Content-Type": "application/json",
        Location: "/api/assets/asset-1"
      }
    }));

    const service = createAssetsService("https://localhost:7235");

    await expect(service.createAsset({
      title: "Published asset",
      markdownContent: "# Content",
      assetTypeId: "type-1",
      domainId: "domain-1",
      useCaseId: "use-case-1",
      toolIds: ["tool-1"],
      tags: []
    })).resolves.toBeUndefined();
  });
});