using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.ToggleAvailability;

public record ToggleAvailabilityResponseDto(bool IsSuccess, DriverAvailabilityStatus Status);
