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
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IPassengerRepository _passengerRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IRideRepository _rideRepository;
    private readonly IPaymentRepository _paymentRepository;

    public GetPlatformStatsHandler(
        IApplicationUserRepository applicationUserRepository,
        IPassengerRepository passengerRepository,
        IDriverRepository driverRepository,
        IRideRepository rideRepository,
        IPaymentRepository paymentRepository)
    {
        _applicationUserRepository = applicationUserRepository;
        _passengerRepository = passengerRepository;
        _driverRepository = driverRepository;
        _rideRepository = rideRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<PlatformStatsDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _applicationUserRepository.CountAsync(cancellationToken: cancellationToken);
        var totalPassengers = await _passengerRepository.CountAsync(cancellationToken: cancellationToken);
        var totalDrivers = await _driverRepository.CountAsync(cancellationToken: cancellationToken);
        var onlineDrivers = await _driverRepository.CountAsync(d => d.AvailabilityStatus == DriverAvailabilityStatus.Online, cancellationToken);
        var totalRides = await _rideRepository.CountAsync(cancellationToken: cancellationToken);
        var completedRides = await _rideRepository.CountAsync(r => r.Status == RideStatus.Completed, cancellationToken);
        var totalRevenue = (await _paymentRepository.FindAsync(p => p.Status == PaymentStatus.Paid, cancellationToken))
            .Sum(p => p.Amount);

        var stats = new PlatformStatsDto(
            totalUsers, totalPassengers, totalDrivers, onlineDrivers, totalRides, completedRides, totalRevenue);

        return stats;
    }
}
