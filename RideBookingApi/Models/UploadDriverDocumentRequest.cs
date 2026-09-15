using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Models;

public sealed class UploadDriverDocumentRequest
{
    public Guid DriverId { get; init; }
    public DocumentType DocumentType { get; init; }
    public IFormFile File { get; init; } = null!;
}
