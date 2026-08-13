import { BookmarkIcon } from "./AssetCardIcons";

interface BookmarkButtonProps {
  assetId: string;
  isBookmarked: boolean;
  onToggle?: (assetId: string, isBookmarked: boolean) => void;
}

export function BookmarkButton({ assetId, isBookmarked, onToggle }: BookmarkButtonProps) {
  return (
    <button
      type="button"
      aria-pressed={isBookmarked}
      aria-label={isBookmarked ? "Remove bookmark" : "Add bookmark"}
      onClick={() => onToggle?.(assetId, !isBookmarked)}
      className="shrink-0 rounded-full p-1.5 text-secondary transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
    >
      <BookmarkIcon filled={isBookmarked} className="size-4" />
    </button>
  );
}
