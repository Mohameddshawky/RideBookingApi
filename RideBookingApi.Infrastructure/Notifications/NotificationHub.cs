using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RideBookingApi.Application.Features.Notifications;

namespace RideBookingApi.Infrastructure.Notifications;

[Authorize]
public class NotificationHub : Hub
{
    public async Task SendNotificationToUser(string userId, NotificationResponse notification, CancellationToken cancellationToken = default)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}
