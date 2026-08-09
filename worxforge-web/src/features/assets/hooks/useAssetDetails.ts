import { useQuery } from "@tanstack/react-query";
import { createAssetsService } from "../services/assetsService";

const assetsService = createAssetsService();

export function useAssetDetails(assetId?: string) {
  return useQuery({
    queryKey: ["assets", "details", assetId],
    enabled: Boolean(assetId),
    queryFn: ({ signal }) => assetsService.getAssetDetails(assetId as string, signal)
  });
}
