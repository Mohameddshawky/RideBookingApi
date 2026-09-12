using Microsoft.EntityFrameworkCore;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Passenger> Passengers { get; }
    DbSet<Driver> Drivers { get; }
    DbSet<DriverDocument> DriverDocuments { get; }
    DbSet<Administrator> Administrators { get; }
    DbSet<Ride> Rides { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
