using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TransactionId)
            .HasMaxLength(200);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(500);

        builder.HasOne(p => p.Passenger)
            .WithMany(passenger => passenger.Payments)
            .HasForeignKey(p => p.PassengerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
