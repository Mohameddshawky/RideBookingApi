using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.CancelRide;

public record CancelRideCommand(Guid RideId, string Reason);

public class CancelRideHandler
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CancelRideHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<CancelRideResponseDto> HandleAsync(CancelRideCommand command, CancellationToken cancellationToken = default)
    {
        var ride = await _context.Rides
            .Include(r => r.Passenger)
            .FirstOrDefaultAsync(r => r.Id == command.RideId, cancellationToken);

        if (ride == null)
        {
            return new CancelRideResponseDto(false, "Ride not found.");
        }

        if (ride.Status == RideStatus.Completed || ride.Status == RideStatus.Cancelled)
        {
            return new CancelRideResponseDto(false, $"Ride cannot be cancelled from status: {ride.Status}");
        }

        ride.Status = RideStatus.Cancelled;
        ride.CancelledAt = DateTime.UtcNow;
        ride.CancellationReason = command.Reason;

        if (ride.DriverId.HasValue)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == ride.DriverId.Value, cancellationToken);
            if (driver != null)
            {
                driver.AvailabilityStatus = DriverAvailabilityStatus.Online;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (ride.Passenger != null)
        {
            await _notificationService.SendNotificationAsync(
                ride.Passenger.UserId,
                NotificationType.RideStatusUpdate,
                "Ride Cancelled",
                $"Your ride has been cancelled. Reason: {command.Reason}",
                cancellationToken);
        }

        return new CancelRideResponseDto(true, "Ride cancelled successfully.");
    }
}
