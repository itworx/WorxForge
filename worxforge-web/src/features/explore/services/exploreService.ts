import { Client } from "../../../api/worxforgeClient";
import type { AssetSummaryDto, AssetSummaryDtoPagedResultDto, LookupItemDto } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetSummary } from "../../assets/models/assets.models";
import type { AssetSearchRequest, AssetSearchResult, ExploreFilters, LookupItem } from "../models/explore.models";

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


function toAssetSearchResult(dto: AssetSummaryDtoPagedResultDto): AssetSearchResult {
  return {
    items: (dto.items ?? []).map(toAssetSummary),
    page: dto.page ?? 1,
    pageSize: dto.pageSize ?? 0,
    totalCount: dto.totalCount ?? 0,
    hasNextPage: dto.hasNextPage ?? false
  };
}

function toLookupItem(dto: LookupItemDto): LookupItem {
  return {
    id: dto.id ?? "",
    name: dto.name ?? ""
  };
}

export interface ExploreService {
  getExploreFilters(signal?: AbortSignal): Promise<ExploreFilters>;
  browseAssets(request: AssetSearchRequest, signal?: AbortSignal): Promise<AssetSearchResult>;
  searchAssets(request: AssetSearchRequest, signal?: AbortSignal): Promise<AssetSearchResult>;
}

export function createExploreService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): ExploreService {
  const client = new Client(baseUrl, createDevAuthenticatedFetch());

  async function browseAssets(request: AssetSearchRequest, signal?: AbortSignal) {
    const dto = await client.browseAssets(
      request.keywords,
      request.assetTypeId,
      request.domainId,
      request.useCaseId,
      request.toolId,
      request.freshness,
      request.sortBy,
      request.page,
      request.pageSize,
      signal
    );
    return toAssetSearchResult(dto);
  }

  async function searchAssets(request: AssetSearchRequest, signal?: AbortSignal) {
    const dto = await client.searchAssets(
      request.keywords,
      request.assetTypeId,
      request.domainId,
      request.useCaseId,
      request.toolId,
      request.freshness,
      request.sortBy,
      request.page,
      request.pageSize,
      signal
    );
    return toAssetSearchResult(dto);
  }

  return {
    async getExploreFilters(signal) {
      const dto = await client.getExploreFilters(signal);
      return {
        assetTypes: (dto.assetTypes ?? []).map(toLookupItem),
        domains: (dto.domains ?? []).map(toLookupItem),
        useCases: (dto.useCases ?? []).map(toLookupItem),
        tools: (dto.tools ?? []).map(toLookupItem)
      };
    },
    browseAssets,
    searchAssets
  };
}


