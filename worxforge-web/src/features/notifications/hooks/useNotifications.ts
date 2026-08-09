import { useCallback, useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { createNotificationsService } from "../services/notificationsService";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { ROUTES } from "../../../constants/routes.constants";
import type { Notification } from "../models/notifications.models";

const PAGE_SIZE = 10;

export function useNotifications() {
  const navigate = useNavigate();
  const notificationsService = useMemo(() => createNotificationsService(), []);
  const { lastNotification, markOneReadLocally, markAllReadLocally } = useNotificationContext();

  const [items, setItems] = useState<Notification[]>([]);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isError, setIsError] = useState(false);
  const [markingIds, setMarkingIds] = useState<Set<string>>(new Set());
  const [isMarkingAll, setIsMarkingAll] = useState(false);

  const loadPage = useCallback(
    async (targetPage: number, signal?: AbortSignal) => {
      setIsLoading(true);
      setIsError(false);
      try {
        const result = await notificationsService.getNotifications({ page: targetPage, pageSize: PAGE_SIZE }, signal);
        setItems(result.items);
        setPage(result.page);
        setTotalCount(result.totalCount);
        setHasNextPage(result.hasNextPage);
      } catch (error) {
        if ((error as Error).name !== "AbortError") {
          setIsError(true);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [notificationsService]
  );

  useEffect(() => {
    const controller = new AbortController();
    void loadPage(page, controller.signal);
    return () => controller.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  // Prepend live-pushed notifications while viewing the first page.
  useEffect(() => {
    if (!lastNotification || page !== 1) {
      return;
    }
    setItems((current) => {
      if (current.some((item) => item.notificationId === lastNotification.notificationId)) {
        return current;
      }
      return [lastNotification, ...current];
    });
    setTotalCount((count) => count + 1);
  }, [lastNotification, page]);

  const markAsRead = useCallback(
    async (notificationId: string) => {
      setMarkingIds((current) => new Set(current).add(notificationId));
      try {
        await notificationsService.markAsRead(notificationId);
        setItems((current) =>
          current.map((item) => (item.notificationId === notificationId ? { ...item, isRead: true } : item))
        );
        markOneReadLocally(notificationId);
      } finally {
        setMarkingIds((current) => {
          const next = new Set(current);
          next.delete(notificationId);
          return next;
        });
      }
    },
    [notificationsService, markOneReadLocally]
  );

  const markAllAsRead = useCallback(async () => {
    setIsMarkingAll(true);
    try {
      await notificationsService.markAllAsRead();
      setItems((current) => current.map((item) => ({ ...item, isRead: true })));
      markAllReadLocally();
    } finally {
      setIsMarkingAll(false);
    }
  }, [notificationsService, markAllReadLocally]);

  const openNotification = useCallback(
    async (notification: Notification) => {
      if (!notification.isRead) {
        await markAsRead(notification.notificationId);
      }

      if (notification.assetId) {
        const target = notification.commentId
          ? `${ROUTES.ASSET(notification.assetId)}#comment-${notification.commentId}`
          : ROUTES.ASSET(notification.assetId);
        navigate(target);
      }
    },
    [markAsRead, navigate]
  );

  return {
    items,
    page,
    pageSize: PAGE_SIZE,
    totalCount,
    hasNextPage,
    isLoading,
    isError,
    markingIds,
    isMarkingAll,
    setPage,
    markAsRead,
    markAllAsRead,
    openNotification,
    refresh: () => loadPage(page)
  };
}


