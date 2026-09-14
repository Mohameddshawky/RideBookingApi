using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IAdministratorRepository : IGenericRepository<Administrator>
{
    Task<Administrator?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Administrator>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
}
