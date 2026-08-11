import { Client } from "../../../api/worxforgeClient";
import { getAccessToken } from "../../../services/tokenService";
import type { GetNotificationsParams, Notification, NotificationList } from "../models/notifications.models";
import { NotificationType } from "../models/notifications.models";

type NotificationDto = {
  notificationId?: string;
  type?: number;
  message?: string;
  assetId?: string;
  commentId?: string;
  isRead?: boolean;
  createdAt?: string;
};

function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.trim().replace(/\/$/, "");
}

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

export interface NotificationsService {
  getNotifications(params: GetNotificationsParams, signal?: AbortSignal): Promise<NotificationList>;
  getUnreadCount(signal?: AbortSignal): Promise<number>;
  markAsRead(notificationId: string, signal?: AbortSignal): Promise<void>;
  markAllAsRead(signal?: AbortSignal): Promise<void>;
}

export function createNotificationsService(baseUrl: string = import.meta.env.VITE_API_BASE_URL): NotificationsService {
  const normalizedBaseUrl = normalizeBaseUrl(baseUrl ?? "");

  const client = new Client(normalizedBaseUrl, {
    async fetch(url, init) {
      const headers = new Headers(init?.headers);
      const accessToken = getAccessToken();
      if (accessToken) {
        headers.set("Authorization", `Bearer ${accessToken}`);
      }

      return fetch(url, { ...init, headers });
    }
  });

  return {
    async getNotifications({ isRead, page, pageSize }, signal) {
      const result = await client.notifications(isRead, page, pageSize, signal);

      return {
        items: (result?.items ?? []).map(toNotification),
        page: result?.page ?? page,
        pageSize: result?.pageSize ?? pageSize,
        totalCount: result?.totalCount ?? 0,
        hasNextPage: result?.hasNextPage ?? false
      };
    },

    async getUnreadCount(signal) {
      const result = await client.unreadCount(signal);
      return result.count ?? 0;
    },

    async markAsRead(notificationId, signal) {
      await client.read(notificationId, signal);
    },

    async markAllAsRead(signal) {
      await client.readAll(signal);
    }
  };
}


