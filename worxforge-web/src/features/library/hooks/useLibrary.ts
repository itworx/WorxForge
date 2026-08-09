import { useQuery, keepPreviousData } from "@tanstack/react-query";
import { createLibraryService } from "../services/libraryService";
import type { SavedAssetsRequest } from "../models/library.models";

const libraryService = createLibraryService();

export function useSavedAssets(request: SavedAssetsRequest) {
  return useQuery({
    queryKey: ["library", "saved", request],
    queryFn: ({ signal }) => libraryService.getSavedAssets(request, signal),
    placeholderData: keepPreviousData
  });
}


