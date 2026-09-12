namespace RideBookingApi.Application.Features.Drivers.AcceptRide;

public record AcceptRideResponseDto(bool IsSuccess, string Message, Guid? RideId);
