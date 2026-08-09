import { useMutation, useQuery } from "@tanstack/react-query";
import type { CreateAssetRequest, UpdateAssetRequest } from "../../../api/worxforgeClient";
import { createAssetsService, type AssetEditorLookups } from "../services/assetsService";

const assetsService = createAssetsService();

export function useAssetEditorLookups() {
  return useQuery({
    queryKey: ["assets", "editor", "lookups"],
    queryFn: ({ signal }) => assetsService.getEditorLookups(signal),
    staleTime: 5 * 60 * 1000
  });
}

export function useAssetForEdit(assetId?: string, lookups?: AssetEditorLookups) {
  return useQuery({
    queryKey: ["assets", "editor", "detail", assetId],
    queryFn: ({ signal }) => assetsService.getAssetForEdit(assetId!, lookups!, signal),
    enabled: Boolean(assetId && lookups)
  });
}

export function useCreateAsset() {
  return useMutation({
    mutationFn: ({ body, signal }: { body: CreateAssetRequest; signal?: AbortSignal }) =>
      assetsService.createAsset(body, signal)
  });
}

export function useUpdateAsset() {
  return useMutation({
    mutationFn: ({ assetId, body, signal }: { assetId: string; body: UpdateAssetRequest; signal?: AbortSignal }) =>
      assetsService.updateAsset(assetId, body, signal)
  });
}

export function useUploadMarkdown() {
  return useMutation({
    mutationFn: ({ file, signal }: { file: File; signal?: AbortSignal }) => assetsService.uploadMarkdown(file, signal)
  });
}


