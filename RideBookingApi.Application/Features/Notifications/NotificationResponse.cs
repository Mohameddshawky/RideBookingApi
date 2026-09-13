using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Notifications;

public record NotificationResponse(
    Guid Id,
    string UserId,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);
