using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Infrastructure.Persistence.Configurations;

public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.ToTable("Administrators");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Department)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.AdminPermissions)
            .HasMaxLength(500);


    }
}
