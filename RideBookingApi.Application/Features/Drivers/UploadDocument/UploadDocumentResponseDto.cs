using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.UploadDocument;

public record UploadDocumentResponseDto(Guid DocumentId, DocumentStatus Status);
