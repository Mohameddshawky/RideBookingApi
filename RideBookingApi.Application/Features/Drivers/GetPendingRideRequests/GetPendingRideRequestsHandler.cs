using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.GetPendingRideRequests;

public record PendingRideRequestDto(
    Guid Id,
    Guid PassengerId,
    string PickupAddress,
    double PickupLatitude,
    double PickupLongitude,
    string DestinationAddress,
    double DestinationLatitude,
    double DestinationLongitude,
    decimal EstimatedPrice,
    DateTime RequestedAt
);

public class GetPendingRideRequestsHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingRideRequestsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PendingRideRequestDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var rides = await _unitOfWork.Rides.GetByStatusAsync(RideStatus.Requested, cancellationToken);

        return rides
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new PendingRideRequestDto(
                r.Id,
                r.PassengerId,
                r.PickupAddress,
                r.PickupLatitude,
                r.PickupLongitude,
                r.DestinationAddress,
                r.DestinationLatitude,
                r.DestinationLongitude,
                r.EstimatedPrice,
                r.RequestedAt))
            .ToList();
    }
}
