using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.ToggleAvailability;

public record ToggleAvailabilityCommand(Guid DriverId, DriverAvailabilityStatus Status);

public class ToggleAvailabilityHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public ToggleAvailabilityHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ToggleAvailabilityResponseDto> HandleAsync(ToggleAvailabilityCommand command, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(command.DriverId, cancellationToken);
        if (driver == null)
        {
            return new ToggleAvailabilityResponseDto(false, command.Status);
        }

        driver.AvailabilityStatus = command.Status;
        _unitOfWork.Drivers.Update(driver);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ToggleAvailabilityResponseDto(true, command.Status);
    }
}
