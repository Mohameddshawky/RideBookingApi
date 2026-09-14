using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IPassengerRepository : IGenericRepository<Passenger>
{
    Task<Passenger?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Passenger>> GetPassengersByRatingAsync(double minRating, CancellationToken cancellationToken = default);
}
