using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.AcceptRide;

public record AcceptRideCommand(Guid DriverId, Guid RideId);

public class AcceptRideHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AcceptRideHandler(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<AcceptRideResponseDto> HandleAsync(AcceptRideCommand command, CancellationToken cancellationToken = default)
    {
        var ride = await _unitOfWork.Rides.GetByIdAsync(command.RideId, cancellationToken);
        if (ride == null)
        {
            return new AcceptRideResponseDto(false, "Ride request not found.", null);
        }

        if (ride.Status != RideStatus.Requested || ride.DriverId != null)
        {
            return new AcceptRideResponseDto(false, "Ride has already been accepted by another driver.", ride.Id);
        }

        var driver = await _unitOfWork.Drivers.GetByIdWithUserAsync(command.DriverId, cancellationToken);
        if (driver == null || driver.AvailabilityStatus != DriverAvailabilityStatus.Online)
        {
            return new AcceptRideResponseDto(false, "Driver is not available or offline.", ride.Id);
        }

        ride.DriverId = command.DriverId;
        ride.Status = RideStatus.DriverAssigned;
        ride.AssignedAt = DateTime.UtcNow;
        driver.AvailabilityStatus = DriverAvailabilityStatus.Busy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var passenger = await _unitOfWork.Passengers.GetByIdAsync(ride.PassengerId, cancellationToken);
        if (passenger != null)
        {
            await _notificationService.SendNotificationAsync(
                passenger.UserId,
                NotificationType.DriverAssigned,
                "Driver Assigned",
                $"Driver {driver.ApplicationUser.FirstName} has accepted your ride request.",
                cancellationToken);
        }

        return new AcceptRideResponseDto(true, "Ride accepted successfully.", ride.Id);
    }
}
