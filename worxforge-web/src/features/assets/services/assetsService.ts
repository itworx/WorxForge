import { Client, type AssetDetailsDto } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import type { AssetDetails } from "../models/assets.models";

function toAssetDetails(dto: AssetDetailsDto): AssetDetails {
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
    dislikeCount: dto.dislikeCount ?? 0,
    commentCount: dto.commentCount ?? 0,
    isBookmarked: dto.isBookmarked ?? false,
    markdownContent: dto.markdownContent ?? "",
    useCaseName: dto.useCaseName ?? "",
    compatibleTools: dto.compatibleTools ?? [],
    version: dto.version,
    authorId: dto.authorId ?? "",
    authorAvatarUrl: dto.authorAvatarUrl,
    viewCount: dto.viewCount ?? 0,
    isFollowing: dto.isFollowing ?? false
  };
}

export interface AssetsService {
  getAssetDetails(assetId: string, signal?: AbortSignal): Promise<AssetDetails>;
}

export function createAssetsService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): AssetsService {
  const client = new Client(baseUrl, createDevAuthenticatedFetch());

  return {
    async getAssetDetails(assetId, signal) {
      const dto = await client.assetsGET2(assetId, signal);
      return toAssetDetails(dto);
    }
  };
}


