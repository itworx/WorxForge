import { createContext, useContext, useEffect, useMemo, useRef, useState } from "react";
import type { PropsWithChildren } from "react";
import { getAccessToken } from "../services/tokenService";
import { createNotificationsService } from "../features/notifications/services/notificationsService";
import { createNotificationHubClient } from "../features/notifications/services/notificationHubService";
import type { NotificationDeliveryMessage } from "../features/notifications/models/notifications.models";

type NotificationContextValue = {
  unreadCount: number;
  /** Most recently pushed live notification; consumers can watch this to prepend it to a list. */
  lastNotification: NotificationDeliveryMessage | null;
  refreshUnreadCount: () => void;
  markOneReadLocally: (notificationId: string) => void;
  markAllReadLocally: () => void;
};

const noop = () => {};

const NotificationContext = createContext<NotificationContextValue>({
  unreadCount: 0,
  lastNotification: null,
  refreshUnreadCount: noop,
  markOneReadLocally: noop,
  markAllReadLocally: noop
});

export function NotificationProvider({ children }: PropsWithChildren) {
  const [unreadCount, setUnreadCount] = useState(0);
  const [lastNotification, setLastNotification] = useState<NotificationDeliveryMessage | null>(null);
  const notificationsService = useMemo(() => createNotificationsService(), []);
  const markedReadIds = useRef(new Set<string>());

  useEffect(() => {
    async function refreshUnreadCount() {
      try {
        setUnreadCount(await notificationsService.getUnreadCount());
      } catch {
        // Best-effort refresh; keep the previously known count on failure.
      }
    }

    // TODO(future work): re-run this effect on real login/logout transitions once AuthContext
    // supports them; for now it connects once on mount whenever a token (real or dev) exists.
    if (!getAccessToken()) {
      return;
    }

    void refreshUnreadCount();

    const hubClient = createNotificationHubClient();
    hubClient.onNotification((message) => {
      setLastNotification(message);
      if (!message.isRead) {
        setUnreadCount((count) => count + 1);
      }
    });
    // Delivery is best-effort: resync the authoritative count whenever the socket reconnects.
    hubClient.onReconnected(() => void refreshUnreadCount());
    void hubClient.start();

    return () => {
      void hubClient.stop();
    };
  }, [notificationsService]);

  const value: NotificationContextValue = {
    unreadCount,
    lastNotification,
    refreshUnreadCount: () => {
      notificationsService.getUnreadCount().then(setUnreadCount).catch(noop);
    },
    markOneReadLocally: (notificationId) => {
      if (markedReadIds.current.has(notificationId)) {
        return;
      }
      markedReadIds.current.add(notificationId);
      setUnreadCount((count) => Math.max(0, count - 1));
    },
    markAllReadLocally: () => setUnreadCount(0)
  };

  return <NotificationContext.Provider value={value}>{children}</NotificationContext.Provider>;
}

export function useNotificationContext(): NotificationContextValue {
  return useContext(NotificationContext);
}


