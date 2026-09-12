namespace RideBookingApi.Application.Features.Rides.EstimateFare;

public record EstimateFareRequestDto(
    double PickupLatitude,
    double PickupLongitude,
    double DestinationLatitude,
    double DestinationLongitude
);

public record FareEstimateResponseDto(
    decimal EstimatedPrice,
    string Currency = "USD"
);
