using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IRideRepository : IGenericRepository<Ride>
{
    Task<IEnumerable<Ride>> GetByPassengerIdAsync(Guid passengerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ride>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ride>> GetByStatusAsync(RideStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ride>> GetActiveRidesAsync(CancellationToken cancellationToken = default);
}
