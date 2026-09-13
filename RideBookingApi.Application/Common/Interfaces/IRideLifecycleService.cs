using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces;

public interface IRideLifecycleService
{
    bool CanTransition(RideStatus currentStatus, RideStatus nextStatus);
    void ApplyTransition(Ride ride, RideStatus nextStatus, DateTime now);
    bool IsTerminal(RideStatus status);
}
