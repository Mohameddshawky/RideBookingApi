using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Infrastructure.Persistence.Repositories;

namespace RideBookingApi.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly IApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(
        IApplicationDbContext context,
        IApplicationUserRepository applicationUserRepository,
        IAdministratorRepository administratorRepository,
        IDriverRepository driverRepository,
        IDriverDocumentRepository driverDocumentRepository,
        IPassengerRepository passengerRepository,
        IRideRepository rideRepository,
        IPaymentRepository paymentRepository,
        INotificationRepository notificationRepository)
    {
        _context = context;

        ApplicationUsers = applicationUserRepository;
        Administrators = administratorRepository;
        Drivers = driverRepository;
        DriverDocuments = driverDocumentRepository;
        Passengers = passengerRepository;
        Rides = rideRepository;
        Payments = paymentRepository;
        Notifications = notificationRepository;
    }

    public IApplicationUserRepository ApplicationUsers { get; }
    public IAdministratorRepository Administrators { get; }
    public IDriverRepository Drivers { get; }
    public IDriverDocumentRepository DriverDocuments { get; }
    public IPassengerRepository Passengers { get; }
    public IRideRepository Rides { get; }
    public IPaymentRepository Payments { get; }
    public INotificationRepository Notifications { get; }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (_repositories.TryGetValue(type, out var repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }

        var newRepository = new GenericRepository<TEntity>(_context);
        _repositories[type] = newRepository;
        return newRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
