using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.UpdateRideStatus;

public record UpdateRideStatusResponseDto(bool IsSuccess, string Message, RideStatus Status);
