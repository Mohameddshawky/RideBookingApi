using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class PassengerRepository : GenericRepository<Passenger>, IPassengerRepository
{
    public PassengerRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<Passenger?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Passenger>> GetPassengersByRatingAsync(double minRating, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(p => p.Rating >= minRating).ToListAsync(cancellationToken);
    }
}
