import { Client } from "../../../api/worxforgeClient";
import type { AssetSummaryDto, DomainSummaryDto } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetSummary } from "../../assets/models/assets.models";
import type { DomainSummary, HomeDashboard } from "../models/home.models";

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

function toDomainSummary(dto: DomainSummaryDto): DomainSummary {
  return {
    domainId: dto.domainId ?? "",
    name: dto.name ?? "",
    assetCount: dto.assetCount ?? 0
  };
}

export interface HomeService {
  getHomeDashboard(signal?: AbortSignal): Promise<HomeDashboard>;
}

export function createHomeService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): HomeService {
  const client = new Client(baseUrl, createDevAuthenticatedFetch());

  return {
    async getHomeDashboard(signal) {
      const dto = await client.getHomeDashboard(signal);

      return {
        usefulThisWeek: (dto.usefulThisWeek ?? []).map(toAssetSummary),
        recentlyUpdated: (dto.recentlyUpdated ?? []).map(toAssetSummary),
        recommendedForYou: (dto.recommendedForYou ?? []).map(toAssetSummary),
        popularAcrossTeams: (dto.popularAcrossTeams ?? []).map(toAssetSummary),
        domainSummaries: (dto.domainSummaries ?? []).map(toDomainSummary)
      };
    }
  };
}

