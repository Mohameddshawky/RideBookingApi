using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByRideIdAsync(Guid rideId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.RideId == rideId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByPassengerIdAsync(Guid passengerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(p => p.PassengerId == passengerId).OrderByDescending(p => p.ProcessedAt ?? DateTime.UtcNow).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(p => p.Status == status).OrderByDescending(p => p.ProcessedAt ?? DateTime.UtcNow).ToListAsync(cancellationToken);
    }
}
