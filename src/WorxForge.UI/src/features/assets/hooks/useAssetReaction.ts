import { useMutation } from "@tanstack/react-query";
import { Client } from "../../../api/worxforgeClient";
import type { ReactToAssetRequest } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "@/api/httpClient";

export type ReactionType = "Like" | "Dislike";

const client = new Client(import.meta.env.VITE_API_BASE_URL, createDevAuthenticatedFetch());

interface ToggleReactionParams {
  assetId: string;
  reactionType: ReactionType;
}

export function useAssetReaction() {
  return useMutation({
    mutationFn: async ({ assetId, reactionType }: ToggleReactionParams) => {
      const body: ReactToAssetRequest = { reactionType };
      await client.reactionPOST(assetId, body);
    }
  });
}