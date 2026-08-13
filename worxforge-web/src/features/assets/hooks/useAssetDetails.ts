import { useQuery } from "@tanstack/react-query";
import { AssetRequestError, createAssetsService } from "../services/assetsService";

const assetsService = createAssetsService();

export function useAssetDetails(assetId?: string) {
  return useQuery({
    queryKey: ["assets", "details", assetId],
    enabled: Boolean(assetId),
    queryFn: ({ signal }) => assetsService.getAssetDetails(assetId as string, signal),
    retry: (failureCount, error) => {
      if (error instanceof AssetRequestError && error.status === 404) {
        return false;
      }
      return failureCount < 1;
    }
  });
}
