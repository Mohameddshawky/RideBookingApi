using Microsoft.AspNetCore.Identity;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public UserRoleType UserRoleType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Refresh token properties for JWT Authentication
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation Properties (1-to-1 Profile relationships)
    public Passenger? PassengerProfile { get; set; }
    public Driver? DriverProfile { get; set; }
    public Administrator? AdministratorProfile { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
