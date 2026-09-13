using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Services;

public class RideLifecycleService : IRideLifecycleService
{
    public bool IsTerminal(RideStatus status)
        => status == RideStatus.Completed || status == RideStatus.Cancelled;

    public bool CanTransition(RideStatus currentStatus, RideStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        if (IsTerminal(currentStatus))
        {
            return false;
        }

        return currentStatus switch
        {
            RideStatus.Requested => nextStatus is RideStatus.DriverAssigned or RideStatus.Cancelled,
            RideStatus.DriverAssigned => nextStatus is RideStatus.DriverArrived or RideStatus.Cancelled,
            RideStatus.DriverArrived => nextStatus is RideStatus.InProgress or RideStatus.Cancelled,
            RideStatus.InProgress => nextStatus is RideStatus.Completed or RideStatus.Cancelled,
            RideStatus.Completed => false,
            RideStatus.Cancelled => false,
            _ => false
        };
    }

    public void ApplyTransition(Ride ride, RideStatus nextStatus, DateTime now)
    {
        if (!CanTransition(ride.Status, nextStatus))
        {
            throw new InvalidOperationException($"Ride cannot transition from {ride.Status} to {nextStatus}.");
        }

        ride.Status = nextStatus;

        switch (nextStatus)
        {
            case RideStatus.DriverAssigned:
                ride.AssignedAt ??= now;
                break;
            case RideStatus.DriverArrived:
                ride.ArrivedAt ??= now;
                break;
            case RideStatus.InProgress:
                ride.StartedAt ??= now;
                break;
            case RideStatus.Completed:
                ride.CompletedAt ??= now;
                ride.FinalPrice ??= ride.EstimatedPrice;
                break;
            case RideStatus.Cancelled:
                ride.CancelledAt ??= now;
                break;
        }
    }
}
