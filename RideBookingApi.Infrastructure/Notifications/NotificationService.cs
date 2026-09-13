using Microsoft.AspNetCore.SignalR;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Features.Notifications;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string userId, NotificationType type, string title, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            ApplicationUser = null!
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        var notificationResponse = new NotificationResponse(
            notification.Id,
            notification.UserId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt);

        try
        {
            await _hubContext.Clients
                .User(userId)
                .SendAsync("ReceiveNotification", notificationResponse, cancellationToken);
        }
        catch
        {
            // Intentionally swallow SignalR delivery failures to avoid breaking business operations.
        }
    }
}
