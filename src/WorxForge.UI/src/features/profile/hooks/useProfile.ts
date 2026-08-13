import { useQuery } from "@tanstack/react-query";
import { createProfileService } from "../services/profileService";

const profileService = createProfileService();

export function useMyProfile() {
  return useQuery({
    queryKey: ["profile", "me"],
    queryFn: ({ signal }) => profileService.getMyProfile(signal)
  });
}

export function usePublicProfile(userId?: string) {
  return useQuery({
    queryKey: ["profile", "public", userId],
    enabled: Boolean(userId),
    queryFn: ({ signal }) => profileService.getPublicProfile(userId as string, signal)
  });
}

export function usePublicProfileAssets(userId: string | undefined, page: number, pageSize: number) {
  return useQuery({
    queryKey: ["profile", "public", "assets", userId, page, pageSize],
    enabled: Boolean(userId),
    queryFn: ({ signal }) => profileService.getPublicProfileAssets(userId as string, page, pageSize, signal)
  });
}


