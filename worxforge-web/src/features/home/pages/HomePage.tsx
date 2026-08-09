import { HeroSection } from "../components/HeroSection";
import { BrowseByArea } from "../components/BrowseByArea";
import { AssetSection } from "../components/AssetSection";
import { useHomeDashboard } from "../hooks/useHomeDashboard";

export function HomePage() {
  const { data, isLoading, isError } = useHomeDashboard();

  return (
    <div className="mx-auto flex flex-col gap-10 px-4 py-10 sm:px-6 lg:py-14">
      <HeroSection />

      {isError && (
        <p className="text-center text-sm text-destructive">Couldn't load the knowledge hub. Please try again later.</p>
      )}

      <BrowseByArea domains={data?.domainSummaries ?? []} isLoading={isLoading} />
      <AssetSection title="Useful this week" assets={data?.usefulThisWeek ?? []} isLoading={isLoading} />
      <AssetSection title="Recently updated" assets={data?.recentlyUpdated ?? []} isLoading={isLoading} />
      <AssetSection title="Recommended for you" assets={data?.recommendedForYou ?? []} isLoading={isLoading} />
      <AssetSection title="Popular across teams" assets={data?.popularAcrossTeams ?? []} isLoading={isLoading} />
    </div>
  );
}


