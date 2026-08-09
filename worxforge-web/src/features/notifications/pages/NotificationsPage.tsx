import { CheckCheck, Loader2 } from "lucide-react";
import { useNotifications } from "../hooks/useNotifications";
import { NotificationListItem } from "../components/NotificationListItem";

export function NotificationsPage() {
  const {
    items,
    page,
    pageSize,
    totalCount,
    hasNextPage,
    isLoading,
    isError,
    markingIds,
    isMarkingAll,
    setPage,
    markAsRead,
    markAllAsRead,
    openNotification
  } = useNotifications();

  const hasUnread = items.some((item) => !item.isRead);
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-4 py-8 sm:px-6 lg:py-10">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-text">Notifications</h1>
          <p className="text-sm text-muted-foreground">Activity across the assets you own, follow, or contribute to.</p>
        </div>

        <button
          type="button"
          disabled={!hasUnread || isMarkingAll}
          onClick={() => void markAllAsRead()}
          className="inline-flex items-center justify-center gap-2 self-start rounded-md border border-input bg-background px-3 py-1.5 text-xs font-medium text-text shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-50 sm:self-auto"
        >
          {isMarkingAll ? (
            <Loader2 aria-hidden="true" className="size-4 animate-spin" />
          ) : (
            <CheckCheck aria-hidden="true" className="size-4" />
          )}
          Mark all read
        </button>
      </div>

      {isError && (
        <p className="rounded-lg border border-destructive/30 bg-destructive/5 p-4 text-sm text-destructive">
          Couldn't load your notifications. Please try again later.
        </p>
      )}

      {isLoading ? (
        <div className="flex justify-center py-12">
          <Loader2 aria-hidden="true" className="size-6 animate-spin text-muted-foreground" />
        </div>
      ) : items.length === 0 && !isError ? (
        <div className="rounded-xl border border-secondary/30 bg-background p-8 text-center text-sm text-muted-foreground shadow-sm">
          You're all caught up. No notifications yet.
        </div>
      ) : items.length > 0 ? (
        <div className="divide-y divide-border overflow-hidden rounded-xl border border-secondary/30 bg-background shadow-sm">
          {items.map((notification) => (
            <NotificationListItem
              key={notification.notificationId}
              notification={notification}
              isMarking={markingIds.has(notification.notificationId)}
              onOpen={(item) => void openNotification(item)}
              onMarkAsRead={(id) => void markAsRead(id)}
            />
          ))}
        </div>
      ) : null}

      {!isLoading && totalCount > 0 && (
        <div className="flex flex-col items-center justify-between gap-3 text-sm text-muted-foreground sm:flex-row">
          <span>
            Page {page} of {totalPages}
          </span>
          <div className="flex gap-2">
            <button
              type="button"
              disabled={page <= 1}
              onClick={() => setPage(page - 1)}
              className="inline-flex items-center justify-center rounded-md border border-input bg-background px-3 py-1.5 text-xs font-medium text-text shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-50"
            >
              Previous
            </button>
            <button
              type="button"
              disabled={!hasNextPage}
              onClick={() => setPage(page + 1)}
              className="inline-flex items-center justify-center rounded-md border border-input bg-background px-3 py-1.5 text-xs font-medium text-text shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground disabled:pointer-events-none disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  );
}



