using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;

namespace RideBookingApi.Application.Features.Drivers.GetEarnings;

public record DriverEarningsDto(Guid DriverId, decimal TotalEarnings, int CompletedRidesCount);

public class GetEarningsHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEarningsHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DriverEarningsDto> HandleAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId, cancellationToken);
        if (driver == null)
        {
            return new DriverEarningsDto(Guid.Empty, 0m, 0);
        }

        var completedRidesCount = await _unitOfWork.Rides.CountAsync(r => r.DriverId == driverId && r.Status == Domain.Enums.RideStatus.Completed, cancellationToken);

        return _mapper.Map<DriverEarningsDto>(driver, opts => opts.Items["CompletedRidesCount"] = completedRidesCount);
    }
}
