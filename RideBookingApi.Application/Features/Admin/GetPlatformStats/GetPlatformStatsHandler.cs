using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
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
    private const string CacheKey = "admin:platform-stats";
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;

    public GetPlatformStatsHandler(IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<PlatformStatsDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var cachedStats = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedStats))
        {
            try
            {
                var deserializedStats = JsonSerializer.Deserialize<PlatformStatsDto>(cachedStats);
                if (deserializedStats is not null)
                {
                    return deserializedStats;
                }
            }
            catch (JsonException)
            {
                await _cache.RemoveAsync(CacheKey, cancellationToken);
            }
        }

        var totalUsers = await _unitOfWork.ApplicationUsers.CountAsync(cancellationToken: cancellationToken);
        var totalPassengers = await _unitOfWork.Passengers.CountAsync(cancellationToken: cancellationToken);
        var totalDrivers = await _unitOfWork.Drivers.CountAsync(cancellationToken: cancellationToken);
        var onlineDrivers = await _unitOfWork.Drivers.CountAsync(d => d.AvailabilityStatus == DriverAvailabilityStatus.Online, cancellationToken);
        var totalRides = await _unitOfWork.Rides.CountAsync(cancellationToken: cancellationToken);
        var completedRides = await _unitOfWork.Rides.CountAsync(r => r.Status == RideStatus.Completed, cancellationToken);
        var totalRevenue = (await _unitOfWork.Payments.FindAsync(p => p.Status == PaymentStatus.Paid, cancellationToken))
            .Sum(p => p.Amount);

        var stats = new PlatformStatsDto(
            totalUsers, totalPassengers, totalDrivers, onlineDrivers, totalRides, completedRides, totalRevenue);

        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(stats), CacheOptions, cancellationToken);

        return stats;
    }
}
