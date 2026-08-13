import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Client } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";

const client = new Client(import.meta.env.VITE_API_BASE_URL, createDevAuthenticatedFetch());

/**
 * Toggle bookmark: `bookmark: true` → POST (save), `false` → DELETE (remove).
 * On success, invalidates the saved-assets list and the asset's own details cache.
 */
export function useBookmark() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ assetId, bookmark }: { assetId: string; bookmark: boolean }) =>
      bookmark ? client.bookmarkPOST(assetId) : client.bookmarkDELETE(assetId),
    onSuccess: (_data, { assetId }) => {
      queryClient.invalidateQueries({ queryKey: ["library", "saved"] });
      queryClient.invalidateQueries({ queryKey: ["assets", "details", assetId] });
    }
  });
}
