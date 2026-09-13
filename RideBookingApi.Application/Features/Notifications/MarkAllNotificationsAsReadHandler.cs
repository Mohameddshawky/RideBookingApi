using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Application.Features.Notifications;

public class MarkAllNotificationsAsReadHandler
{
    private readonly IApplicationDbContext _context;

    public MarkAllNotificationsAsReadHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> HandleAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 0;
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return 0;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }
}
