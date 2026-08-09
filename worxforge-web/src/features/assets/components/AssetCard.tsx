import { Link } from "react-router-dom";
import type { AssetSummary } from "../models/assets.models";
import { ROUTES } from "../../../constants/routes.constants";
import { AssetBadge } from "./AssetBadge";
import { AssetToolTags } from "./AssetToolTags";
import { AssetCardMeta } from "./AssetCardMeta";
import { AssetCardStats } from "./AssetCardStats";
import { BookmarkButton } from "./BookmarkButton";

interface AssetCardProps {
  asset: AssetSummary;
  onToggleBookmark?: (assetId: string, isBookmarked: boolean) => void;
  className?: string;
  showOpenAssetAction?: boolean;
}

export function AssetCard({ asset, onToggleBookmark, className = "", showOpenAssetAction = true }: AssetCardProps) {
  const titleId = `asset-card-title-${asset.assetId}`;

  return (
    <article
      aria-labelledby={titleId}
      className={`flex h-full w-full max-w-xs flex-col gap-2 rounded-xl border border-secondary/30 bg-background p-4 shadow-sm transition-all hover:border-primary hover:shadow-md ${className}`}
    >
      <header className="flex items-start justify-between gap-1.5">
        <AssetBadge label={asset.assetTypeName} variant="primary" />
        <BookmarkButton
          assetId={asset.assetId}
          isBookmarked={asset.isBookmarked}
          onToggle={onToggleBookmark}
        />
      </header>

      <h3 id={titleId} className="min-h-11 text-base leading-snug font-semibold text-text">
        <Link
          to={ROUTES.ASSET(asset.assetId)}
          className="rounded-sm hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
        >
          <span className="line-clamp-2">{asset.title}</span>
        </Link>
      </h3>

      <p className="line-clamp-2 min-h-8 text-xs text-text/70">{asset.description}</p>

      <div className="flex h-11 flex-wrap content-start items-start gap-1 overflow-hidden">
        <AssetBadge label={asset.domainName} variant="secondary" />
        <AssetToolTags tools={asset.tools} />
      </div>

      <div className="mt-auto flex flex-col gap-2 pt-1.5">
        <div className="flex items-end justify-between gap-2">
          <AssetCardMeta authorDisplayName={asset.authorDisplayName} updatedAt={asset.updatedAt} />
          <AssetCardStats reactionCount={asset.reactionCount} commentCount={asset.commentCount} />
        </div>

        {showOpenAssetAction ? (
          <Link
            to={ROUTES.ASSET(asset.assetId)}
            className="mt-3 inline-flex items-center justify-center rounded-lg border border-secondary/30 bg-gray-100 px-3 py-1.5 text-xs font-medium text-black transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            Open asset
          </Link>
        ) : null}
      </div>
    </article>
  );
}
