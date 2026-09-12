using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(string userId, NotificationType type, string title, string message, CancellationToken cancellationToken = default);
}
