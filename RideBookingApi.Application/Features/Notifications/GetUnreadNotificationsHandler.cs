using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Application.Features.Notifications;

public class GetUnreadNotificationsHandler
{
    private readonly IApplicationDbContext _context;

    public GetUnreadNotificationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationResponse>> HandleAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<NotificationResponse>();
        }

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications
            .Select(n => new NotificationResponse(
                n.Id,
                n.UserId,
                n.Type,
                n.Title,
                n.Message,
                n.IsRead,
                n.CreatedAt))
            .ToList();
    }
}
