import { ApiException, Client, type AssetDetailsDto, type AssetSummaryDto, type CreateAssetRequest, type LookupItemDto, type UpdateAssetRequest } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetDetails, AssetSummary, MyAssetsRequest, MyAssetsResult } from "../models/assets.models";

function mapDtoToAssetSummary(dto: AssetSummaryDto): AssetSummary {
  return {
    assetId: dto.assetId ?? "",
    title: dto.title ?? "",
    description: dto.description,
    assetTypeName: dto.assetTypeName ?? "",
    domainName: dto.domainName ?? "",
    tools: dto.tools ?? [],
    authorDisplayName: dto.authorDisplayName ?? "",
    createdAt: dto.createdAt ?? "",
    updatedAt: dto.updatedAt ?? "",
    reactionCount: dto.reactionCount ?? 0,
    commentCount: dto.commentCount ?? 0,
    isBookmarked: dto.isBookmarked ?? false
  };
}

export interface LookupItem {
  id: string;
  name: string;
}

export interface AssetEditorLookups {
  assetTypes: LookupItem[];
  domains: LookupItem[];
  useCases: LookupItem[];
  tools: LookupItem[];
}

export interface AssetEditorData {
  assetId: string;
  title: string;
  description: string;
  markdownContent: string;
  assetTypeId: string;
  domainId: string;
  useCaseId: string;
  toolIds: string[];
  tags: string[];
  preview: AssetSummary;
}

export interface AssetsService {
  getAssetDetails(assetId: string, signal?: AbortSignal): Promise<AssetDetails>;
  getEditorLookups(signal?: AbortSignal): Promise<AssetEditorLookups>;
  getAssetForEdit(assetId: string, lookups: AssetEditorLookups, signal?: AbortSignal): Promise<AssetEditorData>;
  uploadMarkdown(file: File, signal?: AbortSignal): Promise<string>;
  createAsset(body: CreateAssetRequest, signal?: AbortSignal): Promise<void>;
  updateAsset(assetId: string, body: UpdateAssetRequest, signal?: AbortSignal): Promise<void>;
  getMyAssets(request: MyAssetsRequest, signal?: AbortSignal): Promise<MyAssetsResult>;
  deleteAsset(assetId: string, signal?: AbortSignal): Promise<void>;
  setBookmark(assetId: string, isBookmarked: boolean, signal?: AbortSignal): Promise<void>;
}

type UnknownRecord = Record<string, unknown>;

class AssetLookupError extends Error {
  readonly status?: number;

  constructor(message: string, status?: number) {
    super(message);
    this.name = "AssetLookupError";
    this.status = status;
  }
}

class AssetRequestError extends Error {
  readonly status?: number;

  constructor(message: string, status?: number) {
    super(message);
    this.name = "AssetRequestError";
    this.status = status;
  }
}

function isRecord(value: unknown): value is UnknownRecord {
  return typeof value === "object" && value !== null;
}

function toNumberValue(value: unknown): number {
  return typeof value === "number" ? value : 0;
}

function toBooleanValue(value: unknown): boolean {
  return typeof value === "boolean" ? value : false;
}

function mapRecordToAssetDetails(source: UnknownRecord): AssetDetails {
  return {
    assetId: toStringValue(source.assetId),
    title: toStringValue(source.title),
    description: toStringValue(source.description),
    assetTypeName: toStringValue(source.assetTypeName),
    domainName: toStringValue(source.domainName),
    tools: toStringArray(source.tools),
    authorDisplayName: toStringValue(source.authorDisplayName),
    createdAt: toStringValue(source.createdAt),
    updatedAt: toStringValue(source.updatedAt),
    reactionCount: toNumberValue(source.reactionCount),
    dislikeCount: toNumberValue(source.dislikeCount),
    commentCount: toNumberValue(source.commentCount),
    isBookmarked: toBooleanValue(source.isBookmarked),
    markdownContent: toStringValue(source.markdownContent),
    useCaseName: toStringValue(source.useCaseName),
    compatibleTools: toStringArray(source.compatibleTools),
    version: toStringValue(source.version),
    authorId: toStringValue(source.authorId),
    authorAvatarUrl: toStringValue(source.authorAvatarUrl) || undefined,
    viewCount: toNumberValue(source.viewCount),
    isFollowing: toBooleanValue(source.isFollowing)
  };
}

function toLookupItem(dto: LookupItemDto): LookupItem {
  return {
    id: dto.id ?? "",
    name: dto.name ?? ""
  };
}

function toStringValue(value: unknown): string {
  return typeof value === "string" ? value : "";
}

function toStringArray(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }

  return value.filter((entry): entry is string => typeof entry === "string" && entry.trim().length > 0);
}

function normalizeTags(value: unknown): string[] {
  return toStringArray(value).map((tag) => tag.trim()).filter(Boolean);
}

function findLookupId(value: unknown, lookups: LookupItem[]): string {
  const normalizedValue = toStringValue(value).trim();

  if (!normalizedValue) {
    return "";
  }

  const directMatch = lookups.find((lookup) => lookup.id === normalizedValue);
  if (directMatch) {
    return directMatch.id;
  }

  const byName = lookups.find((lookup) => lookup.name.toLowerCase() === normalizedValue.toLowerCase());
  return byName?.id ?? "";
}

function findLookupIds(values: unknown, lookups: LookupItem[]): string[] {
  const asArray = toStringArray(values);
  const ids = asArray.map((value) => findLookupId(value, lookups)).filter(Boolean);
  return Array.from(new Set(ids));
}

function toPreviewSummary(source: UnknownRecord): AssetSummary {
  return {
    assetId: toStringValue(source.assetId) || "preview-asset",
    title: toStringValue(source.title),
    description: toStringValue(source.description),
    assetTypeName: toStringValue(source.assetTypeName),
    domainName: toStringValue(source.domainName),
    tools: toStringArray(source.tools),
    authorDisplayName: toStringValue(source.authorDisplayName) || "You",
    createdAt: toStringValue(source.createdAt) || new Date().toISOString(),
    updatedAt: toStringValue(source.updatedAt) || new Date().toISOString(),
    reactionCount: typeof source.reactionCount === "number" ? source.reactionCount : 0,
    commentCount: typeof source.commentCount === "number" ? source.commentCount : 0,
    isBookmarked: typeof source.isBookmarked === "boolean" ? source.isBookmarked : false
  };
}

function mapAssetDetailsResponse(payload: unknown, lookups: AssetEditorLookups): AssetEditorData {
  if (!isRecord(payload)) {
    throw new Error("Asset details response is invalid.");
  }

  const preview = toPreviewSummary(payload);

  return {
    assetId: toStringValue(payload.assetId),
    title: toStringValue(payload.title),
    description: toStringValue(payload.description),
    markdownContent: toStringValue(payload.markdownContent),
    assetTypeId: findLookupId(payload.assetTypeId ?? payload.assetTypeName, lookups.assetTypes),
    domainId: findLookupId(payload.domainId ?? payload.domainName, lookups.domains),
    useCaseId: findLookupId(payload.useCaseId ?? payload.useCaseName, lookups.useCases),
    toolIds: findLookupIds(payload.toolIds ?? payload.compatibleTools ?? payload.tools, lookups.tools),
    tags: normalizeTags(payload.tags),
    preview: {
      ...preview,
      assetTypeName: preview.assetTypeName || lookups.assetTypes.find((lookup) => lookup.id === findLookupId(payload.assetTypeId ?? payload.assetTypeName, lookups.assetTypes))?.name || "",
      domainName: preview.domainName || lookups.domains.find((lookup) => lookup.id === findLookupId(payload.domainId ?? payload.domainName, lookups.domains))?.name || "",
      tools: preview.tools.length > 0
        ? preview.tools
        : findLookupIds(payload.toolIds ?? payload.compatibleTools ?? payload.tools, lookups.tools)
            .map((toolId) => lookups.tools.find((lookup) => lookup.id === toolId)?.name ?? "")
            .filter(Boolean)
    }
  };
}

async function fetchJson<T>(input: RequestInfo, init?: RequestInit): Promise<T> {
  let response: Response;

  try {
    response = await createDevAuthenticatedFetch().fetch(input, {
      ...init,
      headers: {
        Accept: "application/json",
        ...(init?.headers ?? {})
      }
    });
  } catch {
    throw new AssetRequestError("Could not connect to the WorxForge API. Verify that the API is running and its HTTPS certificate is trusted.");
  }

  if (!response.ok) {
    throw new AssetRequestError(
      response.status === 401
        ? "Your session is not authorized for this action. Sign in again and retry."
        : `The asset request failed (HTTP ${response.status}).`,
      response.status
    );
  }

  try {
    return await response.json() as T;
  } catch {
    throw new AssetRequestError("The API returned an invalid asset response.");
  }
}

async function fetchLookupItems(
  baseUrl: string,
  path: string,
  label: string,
  signal?: AbortSignal
): Promise<LookupItem[]> {
  let response: Response;

  try {
    response = await fetch(`${baseUrl}${path}`, {
      method: "GET",
      signal,
      headers: { Accept: "application/json" }
    });
  } catch {
    throw new AssetLookupError(
      `Could not reach the WorxForge API while loading ${label}. Verify that the API and local proxy are running.`
    );
  }

  if (!response.ok) {
    throw new AssetLookupError(
      response.status === 401
        ? `The API rejected the ${label} request. Sign in again and retry.`
        : `The API could not load ${label} (HTTP ${response.status}).`,
      response.status
    );
  }

  let payload: unknown;
  try {
    payload = await response.json();
  } catch {
    throw new AssetLookupError(`The API returned an invalid response for ${label}.`);
  }

  if (!Array.isArray(payload)) {
    throw new AssetLookupError(`The API returned an invalid response for ${label}.`);
  }

  const items = payload.map((item) => toLookupItem(item as LookupItemDto));
  if (items.some((item) => !item.id || !item.name)) {
    throw new AssetLookupError(`The API returned incomplete values for ${label}.`);
  }

  return items;
}

function toAssetDetails(details: AssetEditorData): AssetDetails {
  return {
    ...details.preview,
    dislikeCount: 0,
    markdownContent: details.markdownContent,
    useCaseName: details.useCaseId,
    compatibleTools: details.preview.tools,
    authorId: "",
    viewCount: 0,
    isFollowing: false
  };
}

export function createAssetsService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): AssetsService {
  const normalizedBaseUrl = baseUrl?.trim().replace(/\/$/, "") ?? "";
  const client = new Client(normalizedBaseUrl, createDevAuthenticatedFetch());

  return {
    async getAssetDetails(assetId, signal) {
      const payload = await fetchJson<unknown>(`${normalizedBaseUrl}/api/assets/${encodeURIComponent(assetId)}`, {
        method: "GET",
        signal
      });

      if (!isRecord(payload)) {
        throw new AssetRequestError("The API returned an invalid asset response.");
      }

      return mapRecordToAssetDetails(payload);
    },

    async getEditorLookups(signal) {
      const [assetTypes, domains, useCases, tools] = await Promise.all([
        fetchLookupItems(normalizedBaseUrl, "/api/assets/types", "asset types", signal),
        fetchLookupItems(normalizedBaseUrl, "/api/assets/domains", "domains", signal),
        fetchLookupItems(normalizedBaseUrl, "/api/assets/use-cases", "use cases", signal),
        fetchLookupItems(normalizedBaseUrl, "/api/assets/tools", "compatible tools", signal)
      ]);

      return {
        assetTypes,
        domains,
        useCases,
        tools
      };
    },

    async getAssetForEdit(assetId, lookups, signal) {
      const payload = await fetchJson<unknown>(`${normalizedBaseUrl}/api/assets/${encodeURIComponent(assetId)}`, {
        method: "GET",
        signal
      });

      return mapAssetDetailsResponse(payload, lookups);
    },

    async uploadMarkdown(file, signal) {
      const formData = new FormData();
      formData.append("file", file, file.name);

      const payload = await fetchJson<unknown>(`${normalizedBaseUrl}/api/assets/upload-markdown`, {
        method: "POST",
        body: formData,
        signal
      });

      if (!isRecord(payload) || typeof payload.markdownContent !== "string") {
        throw new AssetRequestError("The API returned an invalid Markdown upload response.");
      }

      return payload.markdownContent;
    },

    async createAsset(body, signal) {
      try {
        await client.assetsPOST(body, signal);
      } catch (error) {
        if (error && typeof error === "object" && ApiException.isApiException(error) && error.status === 201) {
          return;
        }

        throw error;
      }
    },

    updateAsset(assetId, body, signal) {
      return client.assetsPUT(assetId, body, signal);
    },

    async getMyAssets(request, signal) {
      const dto = await client.myAssets(request.page, request.pageSize, signal);
      return {
        items: (dto.items ?? []).map(mapDtoToAssetSummary),
        page: dto.page ?? request.page,
        pageSize: dto.pageSize ?? request.pageSize,
        totalCount: dto.totalCount ?? 0,
        hasNextPage: dto.hasNextPage ?? false
      };
    },

    deleteAsset(assetId, signal) {
      return client.assetsDELETE2(assetId, signal);
    },

    setBookmark(assetId, isBookmarked, signal) {
      return isBookmarked ? client.bookmarkPOST(assetId, signal) : client.bookmarkDELETE(assetId, signal);
    }
  };
}

export type { AssetDetails };
export { mapAssetDetailsResponse, toAssetDetails };


