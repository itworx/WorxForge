import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { createCommentsService } from "./commentsService";

describe("createCommentsService", () => {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    fetchMock.mockImplementation(async (input, init) => {
      const url = new URL(input.toString(), "http://localhost");

      if (url.pathname === "/api/assets/asset-1/comments" && init?.method === "GET") {
        return new Response(JSON.stringify({
          items: [
            {
              commentId: "comment-1",
              assetId: "asset-1",
              content: "Top level",
              authorDisplayName: "Alice",
              replies: []
            }
          ],
          page: 1,
          pageSize: 20,
          totalCount: 1,
          hasNextPage: false
        }), {
          status: 200,
          headers: { "Content-Type": "application/json" }
        });
      }

      if (url.pathname === "/api/assets/asset-1/comments/comment-1" && init?.method === "GET") {
        return new Response(JSON.stringify({
          commentId: "comment-1",
          assetId: "asset-1",
          content: "Top level",
          authorDisplayName: "Alice",
          replies: [
            {
              commentId: "reply-1",
              assetId: "asset-1",
              parentCommentId: "comment-1",
              content: "Nested reply",
              authorDisplayName: "Bob",
              replies: []
            }
          ]
        }), {
          status: 200,
          headers: { "Content-Type": "application/json" }
        });
      }

      return new Response(null, { status: 200 });
    });

    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    fetchMock.mockReset();
  });

  it("loads paged comments using the generated list endpoint", async () => {
    const service = createCommentsService("http://localhost");

    const result = await service.getAssetComments("asset-1", 1, 20);

    expect(result.totalCount).toBe(1);
    expect(result.items?.[0]?.commentId).toBe("comment-1");
    expect(fetchMock).toHaveBeenCalled();
  });

  it("loads a single thread using the generated thread endpoint", async () => {
    const service = createCommentsService("http://localhost");

    const result = await service.getCommentThread("asset-1", "comment-1");

    expect(result.commentId).toBe("comment-1");
    expect(result.replies?.[0]?.commentId).toBe("reply-1");
  });

  it("creates top-level comments and replies with the expected request body", async () => {
    const service = createCommentsService("http://localhost");

    await service.createComment("asset-1", "Hello world");
    await service.createReply("asset-1", "comment-1", "Hello reply");

    const calls = fetchMock.mock.calls.map(([input, init]) => ({
      url: new URL(input.toString(), "http://localhost").pathname,
      method: init?.method,
      body: init?.body
    }));

    expect(calls).toEqual(
      expect.arrayContaining([
        {
          url: "/api/assets/asset-1/comments",
          method: "POST",
          body: JSON.stringify({ content: "Hello world" })
        },
        {
          url: "/api/assets/asset-1/comments/comment-1/replies",
          method: "POST",
          body: JSON.stringify({ content: "Hello reply" })
        }
      ])
    );
  });
});
