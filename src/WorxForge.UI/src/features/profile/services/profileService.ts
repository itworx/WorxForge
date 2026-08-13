import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetSummary } from "../../assets/models/assets.models";
import type { MyProfile, ProfileStatistics, PublicProfile, PublicProfileAssetsPage } from "../models/profile.models";

type UnknownRecord = Record<string, unknown>;

function asRecord(value: unknown): UnknownRecord {
  return value && typeof value === "object" ? (value as UnknownRecord) : {};
}

function asString(value: unknown): string {
  return typeof value === "string" ? value : "";
}

function asBoolean(value: unknown): boolean {
  return typeof value === "boolean" ? value : false;
}

function asNumber(value: unknown): number {
  return typeof value === "number" && Number.isFinite(value) ? value : 0;
}

function asArray(value: unknown): unknown[] {
  return Array.isArray(value) ? value : [];
}

function toAssetSummary(value: unknown): AssetSummary {
  const dto = asRecord(value);
  const tools = asArray(dto.tools).map((item) => asString(item)).filter(Boolean);

  return {
    assetId: asString(dto.assetId),
    title: asString(dto.title),
    description: asString(dto.description) || undefined,
    assetTypeName: asString(dto.assetTypeName),
    domainName: asString(dto.domainName),
    tools,
    authorDisplayName: asString(dto.authorDisplayName),
    createdAt: asString(dto.createdAt),
    updatedAt: asString(dto.updatedAt),
    reactionCount: asNumber(dto.reactionCount),
    commentCount: asNumber(dto.commentCount),
    isBookmarked: asBoolean(dto.isBookmarked)
  };
}

function extractAssets(payload: UnknownRecord): AssetSummary[] {
  const directAssets = asArray(payload.publishedAssets);
  if (directAssets.length > 0) {
    return directAssets.map(toAssetSummary);
  }

  const nestedItems = asArray(asRecord(payload.assets).items);
  if (nestedItems.length > 0) {
    return nestedItems.map(toAssetSummary);
  }

  const rootItems = asArray(payload.items);
  if (rootItems.length > 0) {
    return rootItems.map(toAssetSummary);
  }

  return [];
}

function toStatistics(payload: UnknownRecord, assets: AssetSummary[], followerCountFromPayload = 0): ProfileStatistics {
  const profileStatistics = asRecord(payload.statistics);
  return {
    totalPublishedAssets: asNumber(profileStatistics.totalPublishedAssets) || assets.length,
    totalFollowers: asNumber(profileStatistics.totalFollowers) || asNumber(payload.followerCount) || followerCountFromPayload,
    totalReactionsReceived:
      asNumber(profileStatistics.totalReactionsReceived) || assets.reduce((sum, asset) => sum + asset.reactionCount, 0)
  };
}

export interface ProfileService {
  getMyProfile(signal?: AbortSignal): Promise<MyProfile>;
  getPublicProfile(userId: string, signal?: AbortSignal): Promise<PublicProfile>;
  getPublicProfileAssets(userId: string, page: number, pageSize: number, signal?: AbortSignal): Promise<PublicProfileAssetsPage>;
}

export function createProfileService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): ProfileService {
  const http = createDevAuthenticatedFetch();
  const normalizedBaseUrl = baseUrl.replace(/\/$/, "");

  async function requestJson(path: string, signal?: AbortSignal): Promise<unknown> {
    const response = await http.fetch(`${normalizedBaseUrl}${path}`, {
      method: "GET",
      signal,
      headers: {
        Accept: "application/json"
      }
    });

    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`);
    }

    const text = await response.text();
    return text ? JSON.parse(text) : {};
  }

  return {
    async getMyProfile(signal) {
      const [profileResult, assetsResult] = await Promise.allSettled([
        requestJson("/api/profile/me", signal),
        requestJson("/api/library/my-assets?Page=1&PageSize=24", signal)
      ]);

      const profilePayload = profileResult.status === "fulfilled" ? asRecord(profileResult.value) : {};
      const fallbackAssetsPayload = assetsResult.status === "fulfilled" ? asRecord(assetsResult.value) : {};

      const profileAssets = extractAssets(profilePayload);
      const fallbackAssets = extractAssets(fallbackAssetsPayload);
      const publishedAssets = profileAssets.length > 0 ? profileAssets : fallbackAssets;

      return {
        userId: asString(profilePayload.userId),
        username: asString(profilePayload.username),
        displayName: asString(profilePayload.displayName),
        email: asString(profilePayload.email),
        emailVerified: asBoolean(profilePayload.emailVerified),
        avatarUrl: asString(profilePayload.avatarUrl) || undefined,
        createdAt: asString(profilePayload.createdAt),
        publishedAssets,
        statistics: toStatistics(profilePayload, publishedAssets)
      };
    },

    async getPublicProfile(userId, signal) {
      const payload = asRecord(await requestJson(`/api/profile/${encodeURIComponent(userId)}`, signal));
      const publishedAssets = extractAssets(payload);
      const followerCount = asNumber(payload.followerCount);

      return {
        userId: asString(payload.userId) || userId,
        displayName: asString(payload.displayName),
        avatarUrl: asString(payload.avatarUrl) || undefined,
        publishedAssets,
        followerCount,
        statistics: toStatistics(payload, publishedAssets, followerCount)
      };
    },

    async getPublicProfileAssets(userId, page, pageSize, signal) {
      const safePage = Math.max(1, page);
      const safePageSize = Math.max(1, pageSize);
      const payload = asRecord(
        await requestJson(
          `/api/profile/${encodeURIComponent(userId)}/assets?page=${safePage}&pageSize=${safePageSize}`,
          signal
        )
      );

      const items = extractAssets(payload);
      const resolvedPage = asNumber(payload.page) || safePage;
      const resolvedPageSize = asNumber(payload.pageSize) || safePageSize;
      const resolvedTotalCount = asNumber(payload.totalCount) || items.length;
      const resolvedHasNextPage =
        typeof payload.hasNextPage === "boolean"
          ? payload.hasNextPage
          : resolvedPage * resolvedPageSize < resolvedTotalCount;

      return {
        items,
        page: resolvedPage,
        pageSize: resolvedPageSize,
        totalCount: resolvedTotalCount,
        hasNextPage: resolvedHasNextPage
      };
    }
  };
}


