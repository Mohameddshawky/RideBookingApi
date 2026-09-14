using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IDriverDocumentRepository : IGenericRepository<DriverDocument>
{
    Task<IEnumerable<DriverDocument>> GetByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DriverDocument>> GetPendingDocumentsAsync(CancellationToken cancellationToken = default);
}
