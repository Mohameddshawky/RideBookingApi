using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;

namespace RideBookingApi.Application.Features.Notifications;

public class MarkAllNotificationsAsReadHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsAsReadHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> HandleAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 0;
        }

        var notifications = (await _unitOfWork.Notifications.GetUnreadByUserIdAsync(userId, cancellationToken)).ToList();

        if (notifications.Count == 0)
        {
            return 0;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        _unitOfWork.Notifications.UpdateRange(notifications);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }
}
