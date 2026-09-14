using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Repositories;

public class AdministratorRepository : GenericRepository<Administrator>, IAdministratorRepository
{
    public AdministratorRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<Administrator?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Administrator>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(a => a.Department == department).ToListAsync(cancellationToken);
    }
}
