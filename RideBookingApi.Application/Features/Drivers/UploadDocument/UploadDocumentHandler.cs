using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.UploadDocument;

public record UploadDocumentCommand(Guid DriverId, DocumentType DocumentType, string DocumentUrl);

public class UploadDocumentHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UploadDocumentHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UploadDocumentResponseDto> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == command.DriverId, cancellationToken);
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

        _context.DriverDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UploadDocumentResponseDto>(document);
    }
}
