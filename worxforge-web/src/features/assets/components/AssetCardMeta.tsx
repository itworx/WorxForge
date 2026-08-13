import { formatDate } from "../../../utils/date.utils";

interface AssetCardMetaProps {
  authorDisplayName: string;
  updatedAt: string;
}

export function AssetCardMeta({ authorDisplayName, updatedAt }: AssetCardMetaProps) {
  return (
    <p className="truncate text-[11px] text-text/60">
      <span>{authorDisplayName}</span>
      <span aria-hidden="true"> &middot; </span>
      <time dateTime={updatedAt}>{formatDate(updatedAt)}</time>
    </p>
  );
}
