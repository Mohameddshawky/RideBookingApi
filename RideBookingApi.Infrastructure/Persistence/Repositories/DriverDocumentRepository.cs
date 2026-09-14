using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class DriverDocumentRepository : GenericRepository<DriverDocument>, IDriverDocumentRepository
{
    public DriverDocumentRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DriverDocument>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(d => d.DriverId == driverId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DriverDocument>> GetPendingDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(d => d.Status == DocumentStatus.Pending).ToListAsync(cancellationToken);
    }
}
