using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class DriverRepository : GenericRepository<Driver>, IDriverRepository
{
    public DriverRepository(IApplicationDbContext context) : base(context)
    {
    }
    public async Task<Driver?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
    public async Task<Driver?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

    public async Task<Driver?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.ApplicationUser)
            .FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber, cancellationToken);
    }

    public async Task<IEnumerable<Driver>> GetByAvailabilityStatusAsync(DriverAvailabilityStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.ApplicationUser)
            .AsNoTracking()
            .Where(d => d.AvailabilityStatus == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.ApplicationUser)
            .AsNoTracking()
            .Where(d => d.AvailabilityStatus == DriverAvailabilityStatus.Online)
            .ToListAsync(cancellationToken);
    }
}
