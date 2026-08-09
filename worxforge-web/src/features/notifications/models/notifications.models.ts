import type { PagedResult } from "../../../models/common.models";

export const NotificationType = {
  CommentOnAsset: 0,
  CommentReply: 1,
  PostLiked: 2,
  PostDisliked: 3,
  Announcement: 4,
  Unknown: -1
} as const;

export type NotificationType = (typeof NotificationType)[keyof typeof NotificationType];

export interface Notification {
  notificationId: string;
  type: NotificationType;
  message: string;
  assetId?: string;
  commentId?: string;
  isRead: boolean;
  createdAt: string;
}

/** Payload pushed by the "ReceiveNotification" SignalR hub event; same shape as Notification. */
export type NotificationDeliveryMessage = Notification;

export interface UnreadNotificationCount {
  count: number;
}

export type NotificationList = PagedResult<Notification>;

export interface GetNotificationsParams {
  isRead?: boolean;
  page: number;
  pageSize: number;
}

