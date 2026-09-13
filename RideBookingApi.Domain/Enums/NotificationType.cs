namespace RideBookingApi.Domain.Enums;

public enum NotificationType
{
    RideStatusUpdate,
    DriverAssigned,
    DriverArrived,
    RideStarted,
    RideCompleted,
    RideCancelled,
    PaymentStatusUpdate,
    PaymentSuccessful,
    PaymentFailed,
    SystemMessage
}
