import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { createExploreService } from "../services/exploreService";
import type { AssetSearchRequest } from "../models/explore.models";

const exploreService = createExploreService();

export function useExploreFilters() {
  return useQuery({
    queryKey: ["explore", "filters"],
    queryFn: ({ signal }) => exploreService.getExploreFilters(signal),
    staleTime: 5 * 60 * 1000
  });
}

/** Uses keyword search when a query is present, otherwise browses all published assets. */
export function useExploreAssets(request: AssetSearchRequest) {
  const hasKeywords = Boolean(request.keywords?.trim());

  return useQuery({
    queryKey: ["explore", "assets", hasKeywords ? "search" : "browse", request],
    queryFn: ({ signal }) =>
      hasKeywords ? exploreService.searchAssets(request, signal) : exploreService.browseAssets(request, signal),
    placeholderData: keepPreviousData
  });
}



