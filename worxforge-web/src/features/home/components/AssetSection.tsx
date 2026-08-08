import { Compass } from "lucide-react";
import { Link } from "react-router-dom";
import { AssetsDashboard } from "../../assets/components/AssetsDashboard";
import type { AssetSummary } from "../../assets/models/assets.models";
import { ROUTES } from "../../../constants/routes.constants";

interface AssetSectionProps {
  title: string;
  assets: AssetSummary[];
  isLoading?: boolean;
}

export function AssetSection({ title, assets, isLoading = false }: AssetSectionProps) {
  return (
    <section>
      <div className="mb-3 flex items-center justify-between">
        <h2 className="text-lg font-semibold">{title}</h2>
        <Link
          to={ROUTES.EXPLORE}
          className="inline-flex h-8 items-center gap-1 rounded-md px-3 text-xs font-medium text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <Compass aria-hidden="true" className="h-4 w-4" />
          Explore all
        </Link>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-[repeat(auto-fit,minmax(200px,1fr))] gap-4">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-52 animate-pulse rounded-xl border border-secondary/30 bg-secondary/10" />
          ))}
        </div>
      ) : (
        <AssetsDashboard assets={assets} />
      )}
    </section>
  );
}
