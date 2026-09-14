using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;

namespace RideBookingApi.Application.Features.Rides.GetPassengerRideHistory;

public record PassengerRideHistoryItemDto(
    Guid RideId,
    string PickupAddress,
    string DestinationAddress,
    decimal EstimatedPrice,
    decimal? FinalPrice,
    string Status,
    string? DriverName,
    DateTime RequestedAt)
{
    public Guid Id => RideId;
}

public record PassengerRideHistoryResponseDto(IReadOnlyList<PassengerRideHistoryItemDto> Rides);

public class GetPassengerRideHistoryHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPassengerRideHistoryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PassengerRideHistoryResponseDto> HandleAsync(Guid passengerId, CancellationToken cancellationToken = default)
    {
        var rides = await _unitOfWork.Rides.FindAsync(r => r.PassengerId == passengerId, cancellationToken);

        var result = rides
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new PassengerRideHistoryItemDto(
                r.Id,
                r.PickupAddress,
                r.DestinationAddress,
                r.EstimatedPrice,
                r.FinalPrice,
                r.Status.ToString(),
                r.Driver == null
                    ? null
                    : r.Driver.ApplicationUser == null
                        ? null
                        : r.Driver.ApplicationUser.FirstName + " " + r.Driver.ApplicationUser.LastName,
                r.RequestedAt))
            .ToList();

        return new PassengerRideHistoryResponseDto(result);
    }
}
