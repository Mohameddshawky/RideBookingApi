using RideBookingApi.Application.Common.Interfaces.Repositories;

namespace RideBookingApi.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IApplicationUserRepository ApplicationUsers { get; }
    IAdministratorRepository Administrators { get; }
    IDriverRepository Drivers { get; }
    IDriverDocumentRepository DriverDocuments { get; }
    IPassengerRepository Passengers { get; }
    IRideRepository Rides { get; }
    IPaymentRepository Payments { get; }
    INotificationRepository Notifications { get; }

    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
