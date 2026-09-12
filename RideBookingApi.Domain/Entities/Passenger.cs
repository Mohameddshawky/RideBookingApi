namespace RideBookingApi.Domain.Entities;

public class Passenger
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public double Rating { get; set; } = 5.0;

    // Navigation properties
    public ICollection<Ride> Rides { get; set; } = new List<Ride>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
