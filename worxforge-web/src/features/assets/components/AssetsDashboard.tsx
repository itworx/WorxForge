import type { AssetSummary } from "../models/assets.models";
import { AssetCard } from "./AssetCard";

interface AssetsDashboardProps {
  assets: AssetSummary[];
  onToggleBookmark?: (assetId: string, isBookmarked: boolean) => void;
  className?: string;
}

export function AssetsDashboard({ assets, onToggleBookmark, className = "" }: AssetsDashboardProps) {
  if (assets.length === 0) {
    return <p className="text-sm text-text/70">No assets to show.</p>;
  }

  return (
    <div
      className={`grid grid-cols-[repeat(auto-fit,minmax(200px,1fr))] gap-4 ${className}`}
    >
      {assets.map((asset) => (
        <AssetCard key={asset.assetId} asset={asset} onToggleBookmark={onToggleBookmark} />
      ))}
    </div>
  );
}
