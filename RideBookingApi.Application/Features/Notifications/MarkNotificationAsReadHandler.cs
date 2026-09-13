using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Application.Features.Notifications;

public class MarkNotificationAsReadHandler
{
    private readonly IApplicationDbContext _context;

    public MarkNotificationAsReadHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(string userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        if (notification.IsRead)
        {
            return true;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
