import { useState } from "react";
import { toast } from "sonner";
import { Pagination } from "../../../components/Pagination";
import { AssetsDashboard } from "../../assets/components/AssetsDashboard";
import { useBookmark } from "../../assets/hooks/useBookmark";
import { useSavedAssets } from "../hooks/useLibrary";

const PAGE_SIZE = 12;

export function LibraryPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading, isFetching, isError } = useSavedAssets({ page, pageSize: PAGE_SIZE });
  const { mutate: toggleBookmark } = useBookmark();

  function handleToggleBookmark(assetId: string, bookmark: boolean) {
    toggleBookmark(
      { assetId, bookmark },
      {
        onError: () => {
          toast.error("Failed to update bookmark. Please try again.");
        }
      }
    );
  }

  const totalCount = data?.totalCount ?? 0;
  const totalPages = totalCount > 0 ? Math.ceil(totalCount / PAGE_SIZE) : 0;

  return (
    <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
      <div>
        <h1 className="text-2xl font-bold">My Library</h1>
        <p className="text-sm text-muted-foreground">Assets you've saved for quick access.</p>
      </div>

      {isError && (
        <p className="text-center text-sm text-destructive">Couldn't load your saved assets. Please try again later.</p>
      )}

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, index) => (
            <div key={index} className="h-52 animate-pulse rounded-xl border border-secondary/30 bg-secondary/10" />
          ))}
        </div>
      ) : (
        <AssetsDashboard assets={data?.items ?? []} onToggleBookmark={handleToggleBookmark} className={isFetching ? "opacity-60" : undefined} />
      )}

      <Pagination
        page={page}
        totalPages={totalPages}
        hasNextPage={data?.hasNextPage ?? false}
        onPageChange={setPage}
        className="pt-2"
      />
    </div>
  );
}


