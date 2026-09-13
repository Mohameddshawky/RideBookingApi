using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.AcceptRide;

public record AcceptRideCommand(Guid DriverId, Guid RideId);

public class AcceptRideHandler
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AcceptRideHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<AcceptRideResponseDto> HandleAsync(AcceptRideCommand command, CancellationToken cancellationToken = default)
    {
        var ride = await _context.Rides.FirstOrDefaultAsync(r => r.Id == command.RideId, cancellationToken);
        if (ride == null)
        {
            return new AcceptRideResponseDto(false, "Ride request not found.", null);
        }

        if (ride.Status != RideStatus.Requested || ride.DriverId != null)
        {
            return new AcceptRideResponseDto(false, "Ride has already been accepted by another driver.", ride.Id);
        }

        var driver = await _context.Drivers
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == command.DriverId, cancellationToken);

        if (driver == null || driver.AvailabilityStatus != DriverAvailabilityStatus.Online)
        {
            return new AcceptRideResponseDto(false, "Driver is not available or offline.", ride.Id);
        }

        ride.DriverId = command.DriverId;
        ride.Status = RideStatus.DriverAssigned;
        ride.AssignedAt = DateTime.UtcNow;
        driver.AvailabilityStatus = DriverAvailabilityStatus.Busy;

        await _context.SaveChangesAsync(cancellationToken);

        var passenger = await _context.Passengers.FirstOrDefaultAsync(p => p.Id == ride.PassengerId, cancellationToken);
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
