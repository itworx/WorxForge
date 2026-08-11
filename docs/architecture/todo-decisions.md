# TODO Decisions

- TODO: AUTH_SESSION lifecycle details.
- TODO: Asset versioning strategy.
- TODO: Recommendation and quality indicator algorithms.
- RESOLVED: Notification delivery mechanism — SignalR `NotificationHub` (API layer only) delivers messages queued via a `System.Threading.Channels`-backed `INotificationQueue`, drained by `NotificationDeliveryBackgroundService`.
- TODO: Search implementation strategy.
