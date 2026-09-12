using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.RequestRide;

public record CreateRideRequestDto(
    Guid PassengerId,
    string PickupAddress,
    double PickupLatitude,
    double PickupLongitude,
    string DestinationAddress,
    double DestinationLatitude,
    double DestinationLongitude
);

public record RideDto(
    Guid Id,
    Guid PassengerId,
    Guid? DriverId,
    string PickupAddress,
    string DestinationAddress,
    decimal EstimatedPrice,
    decimal? FinalPrice,
    RideStatus Status,
    PaymentStatus PaymentStatus,
    DateTime RequestedAt
);
