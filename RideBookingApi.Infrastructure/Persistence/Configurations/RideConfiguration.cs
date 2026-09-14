using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class RideConfiguration : IEntityTypeConfiguration<Ride>
{
    public void Configure(EntityTypeBuilder<Ride> builder)
    {
        builder.ToTable("Rides");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PickupAddress)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(r => r.DestinationAddress)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(r => r.CancellationReason)
            .HasMaxLength(250);

        builder.Property(r => r.EstimatedPrice)
            .HasPrecision(18, 2);

        builder.Property(r => r.FinalPrice)
            .HasPrecision(18, 2);

        builder.HasOne(r => r.Payment)
            .WithOne(p => p.Ride)
            .HasForeignKey<Payment>(p => p.RideId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
