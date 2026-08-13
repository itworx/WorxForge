import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetCommentDto, AssetCommentDtoPagedResultDto, CreateAssetCommentRequest } from "../../../api/worxforgeClient";

function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.trim().replace(/\/$/, "");
}

function buildUrl(baseUrl: string, path: string): string {
  return `${baseUrl}${path}`;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function toCommentsPage(payload: unknown, page: number, pageSize: number): AssetCommentDtoPagedResultDto {
  if (Array.isArray(payload)) {
    return {
      items: payload as AssetCommentDto[],
      page,
      pageSize,
      totalCount: payload.length,
      hasNextPage: false
    };
  }

  if (!isRecord(payload)) {
    return { items: [], page, pageSize, totalCount: 0, hasNextPage: false };
  }

  return {
    items: Array.isArray(payload.items) ? (payload.items as AssetCommentDto[]) : [],
    page: typeof payload.page === "number" ? payload.page : page,
    pageSize: typeof payload.pageSize === "number" ? payload.pageSize : pageSize,
    totalCount: typeof payload.totalCount === "number" ? payload.totalCount : 0,
    hasNextPage: typeof payload.hasNextPage === "boolean" ? payload.hasNextPage : false
  };
}

async function readJson<T>(response: Response, emptyValue: T): Promise<T> {
  const text = await response.text();
  if (!text.trim()) {
    return emptyValue;
  }

  return JSON.parse(text) as T;
}

async function requestJson<T>(url: string, init: RequestInit, emptyValue: T): Promise<T> {
  const response = await createDevAuthenticatedFetch().fetch(url, {
    ...init,
    headers: {
      Accept: "application/json",
      ...(init.headers ?? {})
    }
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed with status ${response.status}.`);
  }

  return readJson(response, emptyValue);
}

export interface CommentsService {
  getAssetComments(assetId: string, page: number, pageSize: number, signal?: AbortSignal): Promise<AssetCommentDtoPagedResultDto>;
  getCommentThread(assetId: string, commentId: string, signal?: AbortSignal): Promise<AssetCommentDto>;
  createComment(assetId: string, content: string, signal?: AbortSignal): Promise<void>;
  createReply(assetId: string, commentId: string, content: string, signal?: AbortSignal): Promise<void>;
  updateComment(assetId: string, commentId: string, content: string, signal?: AbortSignal): Promise<void>;
  deleteComment(assetId: string, commentId: string, signal?: AbortSignal): Promise<void>;
}

export function createCommentsService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): CommentsService {
  const normalizedBaseUrl = normalizeBaseUrl(baseUrl ?? "");

  const toRequestBody = (content: string): CreateAssetCommentRequest => ({ content });

  return {
    async getAssetComments(assetId, page, pageSize, signal) {
      const payload = await requestJson<unknown>(
        buildUrl(normalizedBaseUrl, `/api/assets/${encodeURIComponent(assetId)}/comments?Page=${page}&PageSize=${pageSize}`),
        { method: "GET", signal },
        { items: [], page, pageSize, totalCount: 0, hasNextPage: false }
      );

      return toCommentsPage(payload, page, pageSize);
    },

    async getCommentThread(assetId, commentId, signal) {
      return requestJson<AssetCommentDto>(
        buildUrl(normalizedBaseUrl, `/api/assets/${encodeURIComponent(assetId)}/comments/${encodeURIComponent(commentId)}`),
        { method: "GET", signal },
        { commentId, assetId, replies: [] }
      );
    },

    async createComment(assetId, content, signal) {
      await requestJson<void>(
        buildUrl(normalizedBaseUrl, `/api/assets/${encodeURIComponent(assetId)}/comments`),
        {
          method: "POST",
          signal,
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(toRequestBody(content))
        },
        undefined
      );
    },

    async createReply(assetId, commentId, content, signal) {
      await requestJson<void>(
        buildUrl(normalizedBaseUrl, `/api/assets/${encodeURIComponent(assetId)}/comments/${encodeURIComponent(commentId)}/replies`),
        {
          method: "POST",
          signal,
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(toRequestBody(content))
        },
        undefined
      );
    },

    async updateComment(assetId, commentId, content, signal) {
      await requestJson<void>(
        buildUrl(normalizedBaseUrl, `/api/assets/${encodeURIComponent(assetId)}/comments/${encodeURIComponent(commentId)}`),
        {
          method: "PUT",
          signal,
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(toRequestBody(content))
        },
        undefined
      );
    },

    async deleteComment(assetId, commentId, signal) {
      await requestJson<void>(
        buildUrl(normalizedBaseUrl, `/api/assets/${encodeURIComponent(assetId)}/comments/${encodeURIComponent(commentId)}`),
        { method: "DELETE", signal },
        undefined
      );
    }
  };
}


