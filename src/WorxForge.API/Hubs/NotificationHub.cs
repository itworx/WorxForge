using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WorxForge.Domain.Constants;

namespace WorxForge.API.Hubs;

// No server-invokable methods: clients only receive "ReceiveNotification" pushes from the
// background delivery service. Connection identity comes from the JWT (ClaimTypes.NameIdentifier).
[Authorize(PolicyConstants.AuthenticatedUser)]
public sealed class NotificationHub : Hub;
