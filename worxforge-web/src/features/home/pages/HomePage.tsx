import { useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { HeroSection } from "../components/HeroSection";
import { BrowseByArea } from "../components/BrowseByArea";
import { AssetSection } from "../components/AssetSection";
import { useHomeDashboard } from "../hooks/useHomeDashboard";
import { useBookmark } from "../../assets/hooks/useBookmark";
import type { HomeDashboard } from "../models/home.models";
import type { AssetSummary } from "../../assets/models/assets.models";

function updateBookmarkInDashboard(dashboard: HomeDashboard, assetId: string, bookmark: boolean): HomeDashboard {
  const update = (list: AssetSummary[]) =>
    list.map((a) => (a.assetId === assetId ? { ...a, isBookmarked: bookmark } : a));
  return {
    ...dashboard,
    usefulThisWeek: update(dashboard.usefulThisWeek),
    recentlyUpdated: update(dashboard.recentlyUpdated),
    recommendedForYou: update(dashboard.recommendedForYou),
    popularAcrossTeams: update(dashboard.popularAcrossTeams)
  };
}

export function HomePage() {
  const { data, isLoading, isError } = useHomeDashboard();
  const queryClient = useQueryClient();
  const { mutate: toggleBookmark } = useBookmark();

  function handleToggleBookmark(assetId: string, bookmark: boolean) {
    queryClient.setQueryData<HomeDashboard>(["home", "dashboard"], (old) =>
      old ? updateBookmarkInDashboard(old, assetId, bookmark) : old
    );
    toggleBookmark(
      { assetId, bookmark },
      {
        onError: () => {
          queryClient.setQueryData<HomeDashboard>(["home", "dashboard"], (old) =>
            old ? updateBookmarkInDashboard(old, assetId, !bookmark) : old
          );
          toast.error("Failed to update bookmark. Please try again.");
        }
      }
    );
  }

  return (
    <div className="mx-auto flex flex-col gap-10 px-4 py-10 sm:px-6 lg:py-14">
      <HeroSection />

      {isError && (
        <p className="text-center text-sm text-destructive">Couldn't load the knowledge hub. Please try again later.</p>
      )}

      <BrowseByArea domains={data?.domainSummaries ?? []} isLoading={isLoading} />
      <AssetSection title="Useful this week" assets={data?.usefulThisWeek ?? []} isLoading={isLoading} onToggleBookmark={handleToggleBookmark} />
      <AssetSection title="Recently updated" assets={data?.recentlyUpdated ?? []} isLoading={isLoading} onToggleBookmark={handleToggleBookmark} />
      <AssetSection title="Recommended for you" assets={data?.recommendedForYou ?? []} isLoading={isLoading} onToggleBookmark={handleToggleBookmark} />
      <AssetSection title="Popular across teams" assets={data?.popularAcrossTeams ?? []} isLoading={isLoading} onToggleBookmark={handleToggleBookmark} />
    </div>
  );
}


