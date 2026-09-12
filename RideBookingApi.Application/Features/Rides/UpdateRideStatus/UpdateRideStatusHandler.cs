using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.UpdateRideStatus;

public record UpdateRideStatusCommand(Guid RideId, RideStatus NewStatus);

public class UpdateRideStatusHandler
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public UpdateRideStatusHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<UpdateRideStatusResponseDto> HandleAsync(UpdateRideStatusCommand command, CancellationToken cancellationToken = default)
    {
        var ride = await _context.Rides
            .Include(r => r.Passenger)
            .FirstOrDefaultAsync(r => r.Id == command.RideId, cancellationToken);

        if (ride == null)
        {
            return new UpdateRideStatusResponseDto(false, "Ride not found.", command.NewStatus);
        }

        ride.Status = command.NewStatus;
        var now = DateTime.UtcNow;

        switch (command.NewStatus)
        {
            case RideStatus.DriverArrived:
                ride.ArrivedAt = now;
                break;
            case RideStatus.InProgress:
                ride.StartedAt = now;
                break;
            case RideStatus.Completed:
                ride.CompletedAt = now;
                ride.FinalPrice = ride.EstimatedPrice;
                if (ride.DriverId.HasValue)
                {
                    var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == ride.DriverId.Value, cancellationToken);
                    if (driver != null)
                    {
                        driver.TotalEarnings += ride.FinalPrice.Value;
                        driver.AvailabilityStatus = DriverAvailabilityStatus.Online;
                    }
                }
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (ride.Passenger != null)
        {
            await _notificationService.SendNotificationAsync(
                ride.Passenger.UserId,
                NotificationType.RideStatusUpdate,
                "Ride Status Updated",
                $"Your ride status is now: {command.NewStatus}",
                cancellationToken);
        }

        return new UpdateRideStatusResponseDto(true, $"Ride status updated to {command.NewStatus}.", command.NewStatus);
    }
}
