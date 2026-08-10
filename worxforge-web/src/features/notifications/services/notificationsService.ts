import { Client } from "../../../api/worxforgeClient";
import type { NotificationDto } from "../../../api/worxforgeClient";
import { createDevAuthenticatedFetch } from "../../../api/httpClient";
import { getAccessToken, getCurrentUserId } from "../../../services/tokenService";
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

type NotificationResponse = {
  items?: NotificationDto[];
  page?: number;
  pageSize?: number;
  totalCount?: number;
  hasNextPage?: boolean;
};

type UnreadCountResponse = {
  count?: number;
};

const NOTIFICATIONS_PATH = "/api/notifications";
const UNREAD_COUNT_PATH = "/api/notifications/unread-count";

function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.trim().replace(/\/$/, "");
}

function toPath(url: string): string {
  try {
    return new URL(url, window.location.origin).pathname;
  } catch {
    return url;
  }
}

function isPathMatch(requestUrl: string, expectedPath: string): boolean {
  const path = toPath(requestUrl).replace(/\/$/, "");
  const normalizedExpectedPath = expectedPath.replace(/\/$/, "");
  return path.endsWith(normalizedExpectedPath);
}

async function readJsonClone<T>(response: Response): Promise<T | undefined> {
  const contentType = response.headers.get("Content-Type") ?? "";
  if (!contentType.toLowerCase().includes("application/json")) {
    return undefined;
  }

  const body = await response.clone().text();
  if (!body.trim()) {
    return undefined;
  }

  return JSON.parse(body) as T;
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
  let cachedNotifications: NotificationResponse | undefined;
  let cachedUnreadCount: UnreadCountResponse | undefined;

  const client = new Client(normalizedBaseUrl, {
    async fetch(url, init) {
      const headers = new Headers(init?.headers);
      const accessToken = getAccessToken();
      if (accessToken) {
        headers.set("Authorization", `Bearer ${accessToken}`);
      }

      const response = await fetch(url, { ...init, headers });
      const requestUrl = typeof url === "string" ? url : url instanceof URL ? url.toString() : url.url;

      if (isPathMatch(requestUrl, NOTIFICATIONS_PATH)) {
        cachedNotifications = response.ok ? await readJsonClone<NotificationResponse>(response) : undefined;
      }

      if (isPathMatch(requestUrl, UNREAD_COUNT_PATH)) {
        cachedUnreadCount = response.ok ? await readJsonClone<UnreadCountResponse>(response) : undefined;
      }

      return response;
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


