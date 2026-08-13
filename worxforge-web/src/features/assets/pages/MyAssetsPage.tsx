import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../../../constants/routes.constants";
import { Pagination } from "../../../components/Pagination";
import { AssetsDashboard } from "../components/AssetsDashboard";
import { useMyAssets, useDeleteAsset, useToggleBookmark } from "../hooks/useAssets";

const PAGE_SIZE = 12;

export function MyAssetsPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);

  const { data, isLoading, isFetching, isError } = useMyAssets({ page, pageSize: PAGE_SIZE });
  const deleteAsset = useDeleteAsset();
  const toggleBookmark = useToggleBookmark();

  const totalCount = data?.totalCount ?? 0;
  const totalPages = totalCount > 0 ? Math.ceil(totalCount / PAGE_SIZE) : 0;

  function handleEdit(assetId: string) {
    navigate(ROUTES.MY_ASSETS_EDIT(assetId));
  }

  function handleDelete(assetId: string) {
    if (!window.confirm("Delete this asset? This action cannot be undone.")) {
      return;
    }
    deleteAsset.mutate({ assetId });
  }

  function handleToggleBookmark(assetId: string, isBookmarked: boolean) {
    toggleBookmark.mutate({ assetId, isBookmarked });
  }

  return (
    <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
      <div>
        <h1 className="text-2xl font-bold">My Assets</h1>
        <p className="text-sm text-muted-foreground">Assets you've published to WorxForge.</p>
      </div>

      {isError && (
        <p className="text-center text-sm text-destructive">Couldn't load your assets. Please try again later.</p>
      )}

      {deleteAsset.isError && (
        <p className="text-center text-sm text-destructive">Couldn't delete the asset. Please try again.</p>
      )}

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, index) => (
            <div key={index} className="h-52 animate-pulse rounded-xl border border-secondary/30 bg-secondary/10" />
          ))}
        </div>
      ) : (
        <AssetsDashboard
          assets={data?.items ?? []}
          onToggleBookmark={handleToggleBookmark}
          onEdit={handleEdit}
          onDelete={handleDelete}
          className={isFetching ? "opacity-60" : undefined}
        />
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


