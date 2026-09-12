using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Domain.Entities;

public class Driver
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public string LicenseNumber { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string VehiclePlateNumber { get; set; } = string.Empty;
    public string VehicleColor { get; set; } = string.Empty;

    public DriverAvailabilityStatus AvailabilityStatus { get; set; } = DriverAvailabilityStatus.Offline;
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public double Rating { get; set; } = 5.0;
    public decimal TotalEarnings { get; set; } = 0m;

    // Navigation properties
    public ICollection<DriverDocument> Documents { get; set; } = new List<DriverDocument>();
    public ICollection<Ride> Rides { get; set; } = new List<Ride>();
}
