import { useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { SlidersHorizontal } from "lucide-react";
import { SearchBar } from "../../../components/SearchBar";
import { Pagination } from "../../../components/Pagination";
import { AssetsDashboard } from "../../assets/components/AssetsDashboard";
import { useExploreFilters, useExploreAssets } from "../hooks/useExplore";
import { ExploreFiltersBar, type ExploreFilterValues } from "../components/ExploreFiltersBar";
import type { AssetFreshness, AssetSearchRequest, ExploreSortBy } from "../models/explore.models";

const PAGE_SIZE = 12;

const DEFAULT_FILTER_VALUES: ExploreFilterValues = {
  assetTypeId: "",
  domainId: "",
  useCaseId: "",
  toolId: "",
  freshness: "",
  sortBy: "relevance"
};

export function ExplorePage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialDomainId = searchParams.get("domainId") ?? "";
  const [keywords, setKeywords] = useState(searchParams.get("q") ?? "");
  const [filterValues, setFilterValues] = useState<ExploreFilterValues>({ ...DEFAULT_FILTER_VALUES, domainId: initialDomainId });
  const [showFilters, setShowFilters] = useState(Boolean(initialDomainId));
  const [page, setPage] = useState(1);

  const { data: filters, isLoading: isLoadingFilters } = useExploreFilters();

  const request = useMemo<AssetSearchRequest>(
    () => ({
      keywords: keywords || undefined,
      assetTypeId: filterValues.assetTypeId || undefined,
      domainId: filterValues.domainId || undefined,
      useCaseId: filterValues.useCaseId || undefined,
      toolId: filterValues.toolId || undefined,
      freshness: filterValues.freshness ? (Number(filterValues.freshness) as AssetFreshness) : undefined,
      sortBy: filterValues.sortBy as ExploreSortBy,
      page,
      pageSize: PAGE_SIZE
    }),
    [keywords, filterValues, page]
  );

  const { data, isLoading, isFetching, isError } = useExploreAssets(request);

  function handleSearch(query: string) {
    setKeywords(query);
    setPage(1);
    setSearchParams(query ? { q: query } : {});
  }

  function handleFilterChange(field: keyof ExploreFilterValues, value: string) {
    setFilterValues((previous) => ({ ...previous, [field]: value }));
    setPage(1);
  }

  const totalCount = data?.totalCount ?? 0;
  const totalPages = totalCount > 0 ? Math.ceil(totalCount / PAGE_SIZE) : 0;

  return (
    <div className="mx-auto flex flex-col gap-6 px-4 py-10 sm:px-6 lg:py-14">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <h1 className="text-2xl font-bold">Explore knowledge</h1>
          <p className="text-sm text-muted-foreground">Search and filter across every published asset.</p>
        </div>
        <button
          type="button"
          onClick={() => setShowFilters((previous) => !previous)}
          aria-expanded={showFilters}
          className="inline-flex h-8 items-center justify-center gap-1 rounded-md border border-input bg-background px-3 text-xs font-medium whitespace-nowrap shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <SlidersHorizontal aria-hidden="true" className="mr-1 h-4 w-4" />
          Filters
        </button>
      </div>

      <SearchBar
        defaultValue={keywords}
        placeholder="Search across titles, descriptions, tags, tools, and content…"
        onSearch={handleSearch}
      />

      {showFilters && (
        <ExploreFiltersBar filters={filters} values={filterValues} onChange={handleFilterChange} isLoading={isLoadingFilters} />
      )}

      {isError && (
        <p className="text-center text-sm text-destructive">Couldn't load search results. Please try again later.</p>
      )}

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, index) => (
            <div key={index} className="h-52 animate-pulse rounded-xl border border-secondary/30 bg-secondary/10" />
          ))}
        </div>
      ) : (
        <AssetsDashboard assets={data?.items ?? []} className={isFetching ? "opacity-60" : undefined} />
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



