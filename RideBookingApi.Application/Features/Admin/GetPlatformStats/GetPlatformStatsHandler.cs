using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Admin.GetPlatformStats;

public record PlatformStatsDto(
    int TotalUsers,
    int TotalPassengers,
    int TotalDrivers,
    int OnlineDrivers,
    int TotalRides,
    int CompletedRides,
    decimal TotalRevenue
);

public class GetPlatformStatsHandler
{
    private readonly IApplicationDbContext _context;

    public GetPlatformStatsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformStatsDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalPassengers = await _context.Passengers.CountAsync(cancellationToken);
        var totalDrivers = await _context.Drivers.CountAsync(cancellationToken);
        var onlineDrivers = await _context.Drivers.CountAsync(d => d.AvailabilityStatus == DriverAvailabilityStatus.Online, cancellationToken);
        var totalRides = await _context.Rides.CountAsync(cancellationToken);
        var completedRides = await _context.Rides.CountAsync(r => r.Status == RideStatus.Completed, cancellationToken);
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Paid)
            .SumAsync(p => p.Amount, cancellationToken);

        var stats = new PlatformStatsDto(
            totalUsers, totalPassengers, totalDrivers, onlineDrivers, totalRides, completedRides, totalRevenue);

        return stats;
    }
}
