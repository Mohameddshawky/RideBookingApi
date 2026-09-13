using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.UpdateRideStatus;

public record UpdateRideStatusCommand(Guid RideId, RideStatus NewStatus);

public class UpdateRideStatusHandler
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IRideLifecycleService _rideLifecycleService;

    public UpdateRideStatusHandler(IApplicationDbContext context, INotificationService notificationService, IRideLifecycleService rideLifecycleService)
    {
        _context = context;
        _notificationService = notificationService;
        _rideLifecycleService = rideLifecycleService;
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

        if (!_rideLifecycleService.CanTransition(ride.Status, command.NewStatus))
        {
            return new UpdateRideStatusResponseDto(false, $"Ride cannot transition from {ride.Status} to {command.NewStatus}.", ride.Status);
        }

        var now = DateTime.UtcNow;
        _rideLifecycleService.ApplyTransition(ride, command.NewStatus, now);

        if (command.NewStatus == RideStatus.Completed && ride.DriverId.HasValue)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == ride.DriverId.Value, cancellationToken);
            if (driver != null)
            {
                driver.TotalEarnings += ride.FinalPrice ?? ride.EstimatedPrice;
                driver.AvailabilityStatus = DriverAvailabilityStatus.Online;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (ride.Passenger != null)
        {
            var notificationType = command.NewStatus switch
            {
                RideStatus.DriverAssigned => NotificationType.DriverAssigned,
                RideStatus.DriverArrived => NotificationType.DriverArrived,
                RideStatus.InProgress => NotificationType.RideStarted,
                RideStatus.Completed => NotificationType.RideCompleted,
                RideStatus.Cancelled => NotificationType.RideCancelled,
                _ => NotificationType.RideStatusUpdate
            };

            var title = command.NewStatus switch
            {
                RideStatus.DriverAssigned => "Driver Assigned",
                RideStatus.DriverArrived => "Driver Arrived",
                RideStatus.InProgress => "Ride Started",
                RideStatus.Completed => "Ride Completed",
                RideStatus.Cancelled => "Ride Cancelled",
                _ => "Ride Status Updated"
            };

            await _notificationService.SendNotificationAsync(
                ride.Passenger.UserId,
                notificationType,
                title,
                $"Your ride status is now: {command.NewStatus}",
                cancellationToken);
        }

        return new UpdateRideStatusResponseDto(true, $"Ride status updated to {command.NewStatus}.", command.NewStatus);
    }
}
