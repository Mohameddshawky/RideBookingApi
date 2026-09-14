using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Drivers.UpdateLocation;

public record UpdateDriverLocationCommand(double Latitude, double Longitude);

public class UpdateDriverLocationHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDriverLocationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(string userId, UpdateDriverLocationCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var driver = await _unitOfWork.Drivers.GetByUserIdAsync(userId, cancellationToken);
        if (driver == null)
        {
            return false;
        }

        driver.CurrentLatitude = command.Latitude;
        driver.CurrentLongitude = command.Longitude;
        driver.AvailabilityStatus = DriverAvailabilityStatus.Online;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
