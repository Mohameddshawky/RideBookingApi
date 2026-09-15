using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RideBookingApi.Infrastructure.Persistence;

#nullable disable

namespace RideBookingApi.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260915160000_AddRideConcurrency")]
public partial class AddRideConcurrency : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "Rides",
            type: "rowversion",
            rowVersion: true,
            nullable: false,
            defaultValue: Array.Empty<byte>());
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "Rides");
    }
}
