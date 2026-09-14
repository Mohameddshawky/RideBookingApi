using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class RideRepository : GenericRepository<Ride>, IRideRepository
{
    public RideRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Ride>> GetByPassengerIdAsync(Guid passengerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(r => r.PassengerId == passengerId).OrderByDescending(r => r.RequestedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ride>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(r => r.DriverId == driverId).OrderByDescending(r => r.RequestedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ride>> GetByStatusAsync(RideStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(r => r.Status == status).OrderByDescending(r => r.RequestedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ride>> GetActiveRidesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(r => r.Status != RideStatus.Completed && r.Status != RideStatus.Cancelled).OrderByDescending(r => r.RequestedAt).ToListAsync(cancellationToken);
    }
}
