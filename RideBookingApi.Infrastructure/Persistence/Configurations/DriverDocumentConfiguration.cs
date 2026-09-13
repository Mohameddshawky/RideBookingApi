using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class DriverDocumentConfiguration : IEntityTypeConfiguration<DriverDocument>
{
    public void Configure(EntityTypeBuilder<DriverDocument> builder)
    {
        builder.ToTable("DriverDocuments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.RejectionReason)
            .HasMaxLength(500);


    }
}
