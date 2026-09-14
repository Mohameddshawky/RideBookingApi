using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IApplicationUserRepository : IGenericRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
}
