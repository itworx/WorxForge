import { Link } from "react-router-dom";
import { ROUTES } from "../../../constants/routes.constants";
import type { DomainSummary } from "../models/home.models";

interface BrowseByAreaProps {
  domains: DomainSummary[];
  isLoading?: boolean;
}

export function BrowseByArea({ domains, isLoading = false }: BrowseByAreaProps) {
  if (!isLoading && domains.length === 0) {
    return null;
  }

  return (
    <section>
      <h2 className="mb-3 text-lg font-semibold">Browse by area</h2>
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        {isLoading
          ? Array.from({ length: 8 }).map((_, index) => (
              <div key={index} className="h-17 animate-pulse rounded-xl border border-secondary/30 bg-secondary/10" />
            ))
          : domains.map((domain) => (
              <Link
                key={domain.domainId}
                to={`${ROUTES.EXPLORE}?domain=${encodeURIComponent(domain.name)}`}
                className="rounded-xl border border-secondary/30 bg-white p-4 text-left shadow-sm transition-colors hover:border-primary"
              >
                <div className="text-sm font-semibold">{domain.name}</div>
                <div className="mt-1 text-xs text-muted-foreground">
                  {domain.assetCount} {domain.assetCount === 1 ? "asset" : "assets"}
                </div>
              </Link>
            ))}
      </div>
    </section>
  );
}
