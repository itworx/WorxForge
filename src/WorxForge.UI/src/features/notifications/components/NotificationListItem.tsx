import { Bell, Loader2, Megaphone, MessageSquare, ThumbsDown, ThumbsUp } from "lucide-react";
import type { LucideIcon } from "lucide-react";
import { cn } from "../../../lib/utils";
import { formatRelativeTime } from "../../../utils/date.utils";
import { NotificationType, type Notification } from "../models/notifications.models";

const ICONS_BY_TYPE: Record<NotificationType, LucideIcon> = {
  [NotificationType.CommentOnAsset]: MessageSquare,
  [NotificationType.CommentReply]: MessageSquare,
  [NotificationType.PostLiked]: ThumbsUp,
  [NotificationType.PostDisliked]: ThumbsDown,
  [NotificationType.Announcement]: Megaphone,
  [NotificationType.Unknown]: Bell
};

interface NotificationListItemProps {
  notification: Notification;
  isMarking: boolean;
  onOpen: (notification: Notification) => void;
  onMarkAsRead: (notificationId: string) => void;
}

export function NotificationListItem({ notification, isMarking, onOpen, onMarkAsRead }: NotificationListItemProps) {
  const Icon = ICONS_BY_TYPE[notification.type] ?? Bell;
  const isNavigable = Boolean(notification.assetId);

  const body = (
    <div className={cn("flex items-start gap-3 p-4 transition-colors", !notification.isRead && "bg-primary/5")}>
      <div
        className={cn(
          "grid size-9 shrink-0 place-items-center rounded-full",
          notification.isRead ? "bg-muted text-muted-foreground" : "bg-primary text-primary-foreground"
        )}
      >
        <Icon aria-hidden="true" className="size-4" />
      </div>

      <div className="min-w-0 flex-1">
        <p className="text-sm text-text">{notification.message}</p>
        <p className="mt-1 text-xs text-muted-foreground">{formatRelativeTime(notification.createdAt)}</p>
      </div>

      {!notification.isRead && (
        <button
          type="button"
          disabled={isMarking}
          onClick={(event) => {
            event.preventDefault();
            event.stopPropagation();
            onMarkAsRead(notification.notificationId);
          }}
          className="inline-flex shrink-0 items-center justify-center gap-1 rounded-md border border-input bg-background px-2.5 py-1 text-xs font-medium text-text shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-50"
        >
          {isMarking && <Loader2 aria-hidden="true" className="size-3.5 animate-spin" />}
          Mark read
        </button>
      )}
    </div>
  );

  if (!isNavigable) {
    return <div>{body}</div>;
  }

  return (
    <button
      type="button"
      onClick={() => onOpen(notification)}
      className="block w-full text-left transition-colors hover:bg-accent/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-inset"
    >
      {body}
    </button>
  );
}
