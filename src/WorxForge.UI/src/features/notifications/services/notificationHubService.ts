import * as signalR from "@microsoft/signalr";
import { getAccessToken } from "../../../services/tokenService";
import type { NotificationDeliveryMessage } from "../models/notifications.models";

const RECEIVE_NOTIFICATION_EVENT = "ReceiveNotification";
const RETRY_DELAY_MS = 5000;

export interface NotificationHubClient {
  start(): Promise<void>;
  stop(): Promise<void>;
  onNotification(handler: (message: NotificationDeliveryMessage) => void): void;
  onReconnected(handler: () => void): void;
}

function isUnauthorized(error: unknown): boolean {
  return error instanceof Error && /401|unauthorized/i.test(error.message);
}

const CONSOLE_LOG_BY_LEVEL: Record<signalR.LogLevel, (...args: unknown[]) => void> = {
  [signalR.LogLevel.Trace]: console.debug,
  [signalR.LogLevel.Debug]: console.debug,
  [signalR.LogLevel.Information]: console.info,
  [signalR.LogLevel.Warning]: console.warn,
  [signalR.LogLevel.Error]: console.error,
  [signalR.LogLevel.Critical]: console.error,
  [signalR.LogLevel.None]: () => {}
};

// SignalR's own internal logger prints "stopped during negotiation" at Error level as soon as
// it happens, before our start() catch block ever runs - filter just that expected/benign
// message (e.g. React StrictMode's dev-only double-invoke racing stop() with start()).
const logger: signalR.ILogger = {
  log(logLevel, message) {
    if (/stopped during negotiation/i.test(message)) {
      return;
    }
    CONSOLE_LOG_BY_LEVEL[logLevel](message);
  }
};

/** Builds a SignalR client for the notifications hub, authenticated via the access token query string. */
export function createNotificationHubClient(
  baseUrl: string = import.meta.env.VITE_API_BASE_URL
): NotificationHubClient {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/notifications`, {
      accessTokenFactory: () => getAccessToken() ?? "",
      // Auth is via the bearer token above, not cookies; avoids requiring the server to
      // echo Access-Control-Allow-Credentials: true for the cross-origin dev server.
      withCredentials: false
    })
    .configureLogging(logger)
    .withAutomaticReconnect()
    .build();

  let retryTimeout: ReturnType<typeof setTimeout> | undefined;
  let stopped = false;

  function scheduleRetry() {
    if (stopped || retryTimeout) {
      return;
    }
    retryTimeout = setTimeout(() => {
      retryTimeout = undefined;
      void start();
    }, RETRY_DELAY_MS);
  }

  async function start(): Promise<void> {
    if (stopped || connection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    try {
      await connection.start();
    } catch (error) {
      // If stop() was already called (e.g. effect cleanup racing with an in-flight
      // negotiation, notably React StrictMode's dev-only double-invoke), this rejection
      // is expected noise, not a real connectivity problem - skip the warning/retry.
      if (stopped) {
        return;
      }

      // Covers both an expired/invalid token (future work: retry immediately once real
      // login/logout can supply a fresh token) and the hub simply being unreachable
      // (e.g. backend not running yet) - either way, back off and try again later
      // instead of letting the rejection bubble up as unhandled.
      console.warn(
        isUnauthorized(error) ? "Notification hub unauthorized; will retry." : "Notification hub unreachable; will retry.",
        error
      );
      await connection.stop();
      scheduleRetry();
    }
  }

  connection.onclose((error) => {
    if (error) {
      scheduleRetry();
    }
  });

  return {
    start,
    async stop() {
      stopped = true;
      if (retryTimeout) {
        clearTimeout(retryTimeout);
        retryTimeout = undefined;
      }
      await connection.stop();
    },
    onNotification(handler) {
      connection.on(RECEIVE_NOTIFICATION_EVENT, handler);
    },
    onReconnected(handler) {
      connection.onreconnected(() => handler());
    }
  };
}
