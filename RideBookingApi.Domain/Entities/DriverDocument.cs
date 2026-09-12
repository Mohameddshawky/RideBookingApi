using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Domain.Entities;

public class DriverDocument
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public DocumentType DocumentType { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}
