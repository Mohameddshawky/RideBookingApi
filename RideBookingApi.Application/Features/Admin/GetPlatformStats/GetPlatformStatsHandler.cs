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
    private readonly IUnitOfWork _unitOfWork;

    public GetPlatformStatsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PlatformStatsDto> HandleAsync(CancellationToken cancellationToken = default)
    {
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

        return stats;
    }
}
