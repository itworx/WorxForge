import { Link } from "react-router-dom";
import { AssetsDashboard } from "../../assets/components/AssetsDashboard";
import { ROUTES } from "../../../constants/routes.constants";
import { useMyProfile } from "../hooks/useProfile";

function getInitials(displayName: string): string {
  return displayName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("");
}

export function MyProfilePage() {
  const { data: profile, isLoading, isError } = useMyProfile();

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
        <p className="text-center text-sm text-destructive">Could not load your profile. Please try again later.</p>
      </div>
    );
  }

  return (
    <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
      <section className="rounded-2xl border border-secondary/30 bg-background p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex items-center gap-4">
            {profile.avatarUrl ? (
              <img src={profile.avatarUrl} alt={profile.displayName} className="size-16 rounded-full object-cover" />
            ) : (
              <span className="flex size-16 items-center justify-center rounded-full bg-muted text-lg font-semibold text-text">
                {getInitials(profile.displayName || profile.username || "Me")}
              </span>
            )}

            <div>
              <h1 className="text-2xl font-bold tracking-tight">{profile.displayName || "My profile"}</h1>
              <p className="text-sm text-muted-foreground">@{profile.username || "username"}</p>
              <p className="mt-1 text-xs text-text/70">{profile.email}</p>
            </div>
          </div>

          <Link
            to={ROUTES.SETTINGS}
            className="inline-flex h-9 items-center justify-center rounded-md border border-secondary/30 bg-white px-4 text-sm font-medium text-text transition-colors hover:bg-primary/10 hover:text-primary"
          >
            Edit my profile
          </Link>
        </div>

        <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-3">
          <div className="rounded-xl border border-secondary/30 bg-white p-4">
            <p className="text-xs text-muted-foreground">Published assets</p>
            <p className="mt-1 text-2xl font-semibold text-text">{profile.statistics.totalPublishedAssets}</p>
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
          <h2 className="text-lg font-semibold">My assets</h2>
        </div>
        <AssetsDashboard assets={profile.publishedAssets} />
      </section>
    </div>
  );
}


