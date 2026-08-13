import { useEffect, useRef, useState } from "react";
import { BookmarkIcon, DeleteIcon, EditIcon, MoreIcon } from "./AssetCardIcons";

interface AssetCardMenuProps {
  assetId: string;
  isBookmarked: boolean;
  onToggleBookmark?: (assetId: string, isBookmarked: boolean) => void;
  onEdit?: (assetId: string) => void;
  onDelete?: (assetId: string) => void;
}

/** Kebab menu used on cards that support manage actions (e.g. My Assets: save/edit/delete). */
export function AssetCardMenu({ assetId, isBookmarked, onToggleBookmark, onEdit, onDelete }: AssetCardMenuProps) {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isOpen) return;

    function handleClickOutside(event: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  function selectAction(action: () => void) {
    setIsOpen(false);
    action();
  }

  return (
    <div ref={menuRef} className="relative shrink-0">
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={isOpen}
        aria-label="Asset actions"
        onClick={() => setIsOpen((previous) => !previous)}
        className="rounded-full p-1.5 text-secondary transition-colors hover:bg-primary/10 hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
      >
        <MoreIcon className="size-4" />
      </button>

      {isOpen && (
        <div
          role="menu"
          className="absolute top-full right-0 z-10 mt-1 min-w-32 overflow-hidden rounded-lg border border-secondary/30 bg-background py-1 shadow-md"
        >
          {onToggleBookmark && (
            <button
              type="button"
              role="menuitem"
              onClick={() => selectAction(() => onToggleBookmark(assetId, !isBookmarked))}
              className="flex w-full items-center gap-2 px-3 py-1.5 text-left text-xs text-text transition-colors hover:bg-accent hover:text-accent-foreground"
            >
              <BookmarkIcon filled={isBookmarked} className="size-3.5" />
              {isBookmarked ? "Unsave" : "Save"}
            </button>
          )}
          {onEdit && (
            <button
              type="button"
              role="menuitem"
              onClick={() => selectAction(() => onEdit(assetId))}
              className="flex w-full items-center gap-2 px-3 py-1.5 text-left text-xs text-text transition-colors hover:bg-accent hover:text-accent-foreground"
            >
              <EditIcon className="size-3.5" />
              Edit
            </button>
          )}
          {onDelete && (
            <button
              type="button"
              role="menuitem"
              onClick={() => selectAction(() => onDelete(assetId))}
              className="flex w-full items-center gap-2 px-3 py-1.5 text-left text-xs text-destructive transition-colors hover:bg-destructive/10"
            >
              <DeleteIcon className="size-3.5" />
              Delete
            </button>
          )}
        </div>
      )}
    </div>
  );
}
