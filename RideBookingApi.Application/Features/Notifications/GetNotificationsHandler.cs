using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;

namespace RideBookingApi.Application.Features.Notifications;

public class GetNotificationsHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationResponse>> HandleAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<NotificationResponse>();
        }

        var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId, cancellationToken);

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
