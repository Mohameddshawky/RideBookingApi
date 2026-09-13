using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Services;

public class DriverMatchingService : IDriverMatchingService
{
    private readonly IApplicationDbContext _context;

    public DriverMatchingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Driver?> FindBestDriverAsync(double pickupLatitude, double pickupLongitude, CancellationToken cancellationToken = default)
    {
        var drivers = await _context.Drivers
            .Include(d => d.ApplicationUser)
            .Where(d => d.AvailabilityStatus == DriverAvailabilityStatus.Online)
            .ToListAsync(cancellationToken);

        return drivers
            .Where(d => d.CurrentLatitude.HasValue && d.CurrentLongitude.HasValue)
            .OrderBy(d => CalculateDistance(pickupLatitude, pickupLongitude, d.CurrentLatitude!.Value, d.CurrentLongitude!.Value))
            .FirstOrDefault();
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var earthRadius = 6371.0;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadius * c;
    }

    private static double ToRadians(double value)
    {
        return value * Math.PI / 180.0;
    }
}
