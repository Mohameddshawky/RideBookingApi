using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Tests.Infrastructure;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverDocument> DriverDocuments => Set<DriverDocument>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<Ride> Rides => Set<Ride>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<RideBookingApi.Domain.Entities.Notification> Notifications => Set<RideBookingApi.Domain.Entities.Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.ApplicationUser)
            .WithOne(u => u.PassengerProfile)
            .HasForeignKey<Passenger>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Driver>()
            .HasOne(d => d.ApplicationUser)
            .WithOne(u => u.DriverProfile)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Administrator>()
            .HasOne(a => a.ApplicationUser)
            .WithOne(u => u.AdministratorProfile)
            .HasForeignKey<Administrator>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Notifications)
            .WithOne(n => n.ApplicationUser)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
