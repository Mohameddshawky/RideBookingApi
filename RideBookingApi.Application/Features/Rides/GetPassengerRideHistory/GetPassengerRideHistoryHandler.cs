using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;

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
    private readonly IApplicationDbContext _context;

    public GetPassengerRideHistoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PassengerRideHistoryResponseDto> HandleAsync(Guid passengerId, CancellationToken cancellationToken = default)
    {
        var rides = await _context.Rides
            .Include(r => r.Driver)
            .ThenInclude(d => d.ApplicationUser)
            .Where(r => r.PassengerId == passengerId)
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
            .ToListAsync(cancellationToken);

        return new PassengerRideHistoryResponseDto(rides);
    }
}
