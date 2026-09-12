using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
