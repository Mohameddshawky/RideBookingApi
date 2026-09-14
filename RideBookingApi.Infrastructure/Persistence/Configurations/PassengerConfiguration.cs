using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> builder)
    {
        builder.ToTable("Passengers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasMany(p => p.Rides)
            .WithOne(r => r.Passenger)
            .HasForeignKey(r => r.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Payments)
            .WithOne(pay => pay.Passenger)
            .HasForeignKey(pay => pay.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
