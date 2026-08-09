import type { PagedRequest, PagedResult } from "../../../models/common.models";

export interface AssetSummary {
  assetId: string;
  title: string;
  description?: string;
  assetTypeName: string;
  domainName: string;
  tools: string[];
  authorDisplayName: string;
  createdAt: string;
  updatedAt: string;
  reactionCount: number;
  commentCount: number;
  isBookmarked: boolean;
}

export interface AssetDetails extends AssetSummary {
  dislikeCount: number;
  markdownContent: string;
  useCaseName: string;
  compatibleTools: string[];
  version?: string;
  authorId: string;
  authorAvatarUrl?: string;
  viewCount: number;
  isFollowing: boolean;
}

export interface AssetComment {
  commentId: string;
  assetId: string;
  parentCommentId?: string;
  authorDisplayName: string;
  authorAvatarUrl?: string;
  content: string;
  createdAt: string;
  replies: AssetComment[];
}

export interface MyAsset {
  assetId: string;
  title: string;
  assetTypeName: string;
  version?: string;
  createdAt: string;
  updatedAt: string;
  viewCount: number;
  bookmarkCount: number;
  commentCount: number;
  reactionCount: number;
}

export interface CreateAssetRequest {
  title: string;
  description?: string;
  assetTypeId: string;
  domainId: string;
  useCaseId: string;
  toolIds: string[];
  tags: string[];
  version?: string;
  markdownContent: string;
}

export interface UpdateAssetRequest extends CreateAssetRequest {
  assetId: string;
}

export type MyAssetsRequest = PagedRequest;

export type MyAssetsResult = PagedResult<AssetSummary>;
