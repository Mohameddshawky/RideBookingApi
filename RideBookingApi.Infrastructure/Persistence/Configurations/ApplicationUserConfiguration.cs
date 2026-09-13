using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Email)
            .HasMaxLength(256);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.ApplicationUser)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.PassengerProfile)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey<Passenger>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.DriverProfile)
            .WithOne(d => d.ApplicationUser)
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.AdministratorProfile)
            .WithOne(a => a.ApplicationUser)
            .HasForeignKey<Administrator>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
