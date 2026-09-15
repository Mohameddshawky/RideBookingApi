using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;

namespace RideBookingApi.Application.Features.Drivers.GetEarnings;

public record DriverEarningsDto(Guid DriverId, decimal TotalEarnings, int CompletedRidesCount);

public class GetEarningsHandler
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEarningsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DriverEarningsDto> HandleAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (driver == null)
        {
            return new DriverEarningsDto(Guid.Empty, 0m, 0);
        }

        var completedRidesCount = await _unitOfWork.Rides.CountAsync(r => r.DriverId == driverId && r.Status == Domain.Enums.RideStatus.Completed, cancellationToken);

        return new DriverEarningsDto(driver.Id, driver.TotalEarnings, completedRidesCount);
    }
}
