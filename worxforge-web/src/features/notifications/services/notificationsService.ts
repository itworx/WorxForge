import { Client } from "../../../api/worxforgeClient";
import type { NotificationDto } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import { getCurrentUserId } from "../../../services/tokenService";
import type { GetNotificationsParams, Notification, NotificationList } from "../models/notifications.models";
import { NotificationType } from "../models/notifications.models";

function toNotificationType(type: NotificationDto["type"]): NotificationType {
  return (Object.values(NotificationType) as number[]).includes(type as number)
    ? (type as NotificationType)
    : NotificationType.Unknown;
}

function toNotification(dto: NotificationDto): Notification {
  return {
    notificationId: dto.notificationId ?? "",
    type: toNotificationType(dto.type),
    message: dto.message ?? "",
    assetId: dto.assetId,
    commentId: dto.commentId,
    isRead: dto.isRead ?? false,
    createdAt: dto.createdAt ?? ""
  };
}

function requireCurrentUserId(): string {
  const userId = getCurrentUserId();
  if (!userId) {
    throw new Error("Cannot call the notifications API without an authenticated user.");
  }
  return userId;
}

export interface NotificationsService {
  getNotifications(params: GetNotificationsParams, signal?: AbortSignal): Promise<NotificationList>;
  getUnreadCount(signal?: AbortSignal): Promise<number>;
  markAsRead(notificationId: string, signal?: AbortSignal): Promise<void>;
  markAllAsRead(signal?: AbortSignal): Promise<void>;
}

export function createNotificationsService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): NotificationsService {
  const client = new Client(baseUrl, createDevAuthenticatedFetch());

  return {
    async getNotifications({ isRead, page, pageSize }, signal) {
      const userId = requireCurrentUserId();
      const result = await client.notifications(userId, isRead, page, pageSize, signal);

      return {
        items: (result.items ?? []).map(toNotification),
        page: result.page ?? page,
        pageSize: result.pageSize ?? pageSize,
        totalCount: result.totalCount ?? 0,
        hasNextPage: result.hasNextPage ?? false
      };
    },

    async getUnreadCount(signal) {
      const userId = requireCurrentUserId();
      const result = await client.unreadCount(userId, signal);
      return result.count ?? 0;
    },

    async markAsRead(notificationId, signal) {
      const userId = requireCurrentUserId();
      await client.read({ userId, notificationId }, signal);
    },

    async markAllAsRead(signal) {
      const userId = requireCurrentUserId();
      await client.readAll({ userId }, signal);
    }
  };
}


