namespace RideBookingApi.Domain.Entities;

public class Administrator
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public string Department { get; set; } = string.Empty;
    public string? AdminPermissions { get; set; }
}
