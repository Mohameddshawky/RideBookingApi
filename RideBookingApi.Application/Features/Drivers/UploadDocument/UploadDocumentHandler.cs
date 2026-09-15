using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.UploadDocument;

public record UploadDocumentCommand(Guid DriverId, DocumentType DocumentType, string DocumentUrl);

public class UploadDocumentHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public UploadDocumentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadDocumentResponseDto> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(command.DriverId, cancellationToken);
        if (driver == null)
        {
            return new UploadDocumentResponseDto(Guid.Empty, DocumentStatus.Pending);
        }

        var document = new DriverDocument
        {
            Id = Guid.NewGuid(),
            DriverId = command.DriverId,
            DocumentType = command.DocumentType,
            DocumentUrl = command.DocumentUrl,
            Status = DocumentStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.DriverDocuments.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UploadDocumentResponseDto(document.Id, document.Status);
    }
}
