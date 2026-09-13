using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Common.Interfaces;

public interface IDriverMatchingService
{
    Task<Driver?> FindBestDriverAsync(double pickupLatitude, double pickupLongitude, CancellationToken cancellationToken = default);
}
