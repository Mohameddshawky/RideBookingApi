using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.VehicleModel)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.VehiclePlateNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.VehicleColor)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.TotalEarnings)
            .HasPrecision(18, 2);

        builder.HasMany(d => d.Documents)
            .WithOne(doc => doc.Driver)
            .HasForeignKey(doc => doc.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Rides)
            .WithOne(r => r.Driver)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
