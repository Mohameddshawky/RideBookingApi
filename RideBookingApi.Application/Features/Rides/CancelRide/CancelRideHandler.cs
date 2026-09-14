using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.CancelRide;

public record CancelRideCommand(Guid RideId, string Reason);

public class CancelRideHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IRideLifecycleService _rideLifecycleService;

    public CancelRideHandler(IUnitOfWork unitOfWork, INotificationService notificationService, IRideLifecycleService rideLifecycleService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _rideLifecycleService = rideLifecycleService;
    }

    public async Task<CancelRideResponseDto> HandleAsync(CancelRideCommand command, CancellationToken cancellationToken = default)
    {
        var ride = await _unitOfWork.Rides.GetByIdAsync(command.RideId, cancellationToken);

        if (ride == null)
        {
            return new CancelRideResponseDto(false, "Ride not found.");
        }

        if (!_rideLifecycleService.CanTransition(ride.Status, RideStatus.Cancelled))
        {
            return new CancelRideResponseDto(false, $"Ride cannot be cancelled from status: {ride.Status}");
        }

        _rideLifecycleService.ApplyTransition(ride, RideStatus.Cancelled, DateTime.UtcNow);
        ride.CancellationReason = command.Reason;

        if (ride.DriverId.HasValue)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(ride.DriverId.Value, cancellationToken);
            if (driver != null)
            {
                driver.AvailabilityStatus = DriverAvailabilityStatus.Online;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var passenger = await _unitOfWork.Passengers.GetByIdAsync(ride.PassengerId, cancellationToken);
        if (passenger != null)
        {
            await _notificationService.SendNotificationAsync(
                passenger.UserId,
                NotificationType.RideCancelled,
                "Ride Cancelled",
                $"Your ride has been cancelled. Reason: {command.Reason}",
                cancellationToken);
        }

        return new CancelRideResponseDto(true, "Ride cancelled successfully.");
    }
}
