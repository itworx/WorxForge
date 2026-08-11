import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Pagination } from "../../../components/Pagination";
import { AssetsDashboard } from "../../assets/components/AssetsDashboard";
import { usePublicProfile, usePublicProfileAssets } from "../hooks/useProfile";

const PAGE_SIZE = 12;

function getInitials(displayName: string): string {
  return displayName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("");
}

export function PublicProfilePage() {
  const { userId } = useParams<{ userId: string }>();
  const [page, setPage] = useState(1);
  const { data: profile, isLoading, isError } = usePublicProfile(userId);
  const {
    data: publicAssets,
    isLoading: isAssetsLoading,
    isFetching: isAssetsFetching,
    isError: isAssetsError
  } = usePublicProfileAssets(userId, page, PAGE_SIZE);

  useEffect(() => {
    setPage(1);
  }, [userId]);

  if (isLoading) {
    return (
      <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
        <section className="rounded-2xl border border-secondary/30 bg-background p-6 shadow-sm">
          <div className="h-24 animate-pulse rounded-xl bg-secondary/20" />
        </section>
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
        <p className="text-center text-sm text-destructive">Could not load this user profile. Please try again later.</p>
      </div>
    );
  }

  const totalPages = (publicAssets?.totalCount ?? 0) > 0 ? Math.ceil((publicAssets?.totalCount ?? 0) / PAGE_SIZE) : 0;
  const publishedAssetsCount = publicAssets?.totalCount ?? profile.statistics.totalPublishedAssets;

  return (
    <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
      <section className="rounded-2xl border border-secondary/30 bg-background p-6 shadow-sm">
        <div className="flex items-center gap-4">
          {profile.avatarUrl ? (
            <img src={profile.avatarUrl} alt={profile.displayName} className="size-16 rounded-full object-cover" />
          ) : (
            <span className="flex size-16 items-center justify-center rounded-full bg-muted text-lg font-semibold text-text">
              {getInitials(profile.displayName || "User")}
            </span>
          )}

          <div>
            <h1 className="text-2xl font-bold tracking-tight">{profile.displayName || "User profile"}</h1>
            <p className="text-sm text-muted-foreground">{profile.followerCount} followers</p>
          </div>
        </div>

        <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-3">
          <div className="rounded-xl border border-secondary/30 bg-white p-4">
            <p className="text-xs text-muted-foreground">Published assets</p>
            <p className="mt-1 text-2xl font-semibold text-text">{publishedAssetsCount}</p>
          </div>
          <div className="rounded-xl border border-secondary/30 bg-white p-4">
            <p className="text-xs text-muted-foreground">Followers</p>
            <p className="mt-1 text-2xl font-semibold text-text">{profile.statistics.totalFollowers}</p>
          </div>
          <div className="rounded-xl border border-secondary/30 bg-white p-4">
            <p className="text-xs text-muted-foreground">Reactions received</p>
            <p className="mt-1 text-2xl font-semibold text-text">{profile.statistics.totalReactionsReceived}</p>
          </div>
        </div>
      </section>

      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Published assets</h2>
        </div>

        {isAssetsError && (
          <p className="mb-4 text-sm text-destructive">Could not load this user's assets. Please try again later.</p>
        )}

        {isAssetsLoading ? (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {Array.from({ length: 8 }).map((_, index) => (
              <div key={index} className="h-52 animate-pulse rounded-xl border border-secondary/30 bg-secondary/10" />
            ))}
          </div>
        ) : (
          <AssetsDashboard assets={publicAssets?.items ?? []} className={isAssetsFetching ? "opacity-60" : undefined} />
        )}

        <Pagination
          page={page}
          totalPages={totalPages}
          hasNextPage={publicAssets?.hasNextPage ?? false}
          onPageChange={setPage}
          className="pt-4"
        />
      </section>
    </div>
  );
}


