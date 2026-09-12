using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.ToggleAvailability;

public record ToggleAvailabilityCommand(Guid DriverId, DriverAvailabilityStatus Status);

public class ToggleAvailabilityHandler
{
    private readonly IApplicationDbContext _context;

    public ToggleAvailabilityHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ToggleAvailabilityResponseDto> HandleAsync(ToggleAvailabilityCommand command, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == command.DriverId, cancellationToken);
        if (driver == null)
        {
            return new ToggleAvailabilityResponseDto(false, command.Status);
        }

        driver.AvailabilityStatus = command.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return new ToggleAvailabilityResponseDto(true, command.Status);
    }
}
