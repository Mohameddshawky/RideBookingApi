using Microsoft.AspNetCore.Identity;

namespace RideBookingApi.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
