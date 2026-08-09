import { Client } from "../../../api/worxforgeClient";
import type { AssetSummaryDto } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetSummary } from "../../assets/models/assets.models";
import type { SavedAssetsRequest, SavedAssetsResult } from "../models/library.models";

function toAssetSummary(dto: AssetSummaryDto): AssetSummary {
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

export interface LibraryService {
  getSavedAssets(request: SavedAssetsRequest, signal?: AbortSignal): Promise<SavedAssetsResult>;
}

export function createLibraryService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): LibraryService {
  const client = new Client(baseUrl, createDevAuthenticatedFetch());

  return {
    async getSavedAssets(request, signal) {
      const dto = await client.saved(request.page, request.pageSize, signal);
      return {
        items: (dto.items ?? []).map(toAssetSummary),
        page: dto.page ?? request.page,
        pageSize: dto.pageSize ?? request.pageSize,
        totalCount: dto.totalCount ?? 0,
        hasNextPage: dto.hasNextPage ?? false
      };
    }
  };
}


