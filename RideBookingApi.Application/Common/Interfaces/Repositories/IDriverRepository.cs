using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IDriverRepository : IGenericRepository<Driver>
{    Task<Driver?> GetByIdWithUserAsync(Guid id, CancellationToken cancellationToken = default);    Task<Driver?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Driver?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Driver>> GetByAvailabilityStatusAsync(DriverAvailabilityStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Driver>> GetAvailableDriversAsync(CancellationToken cancellationToken = default);
}
