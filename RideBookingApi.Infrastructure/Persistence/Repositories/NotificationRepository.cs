using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(n => n.UserId == userId && !n.IsRead).OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _dbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
    }
}
