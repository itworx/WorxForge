import type { AssetSummary } from "../../assets/models/assets.models";
import type { PagedResult } from "../../../models/common.models";

export interface LookupItem {
  id: string;
  name: string;
}

export interface ExploreFilters {
  assetTypes: LookupItem[];
  domains: LookupItem[];
  useCases: LookupItem[];
  tools: LookupItem[];
}

/** Mirrors the API's `AssetFreshness` enum. */
export type AssetFreshness = 0 | 1 | 2;

/** Mirrors the API's `ExploreSortByConstants`. */
export type ExploreSortBy = "relevance" | "helpful" | "added" | "updated" | "discussed";

export interface AssetSearchRequest {
  keywords?: string;
  assetTypeId?: string;
  domainId?: string;
  useCaseId?: string;
  toolId?: string;
  freshness?: AssetFreshness;
  sortBy?: ExploreSortBy;
  page: number;
  pageSize: number;
}

export type AssetSearchResult = PagedResult<AssetSummary>;
