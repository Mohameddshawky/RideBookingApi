using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Application.Features.Drivers.GetEarnings;

public record DriverEarningsDto(Guid DriverId, decimal TotalEarnings, int CompletedRidesCount);

public class GetEarningsHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEarningsHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DriverEarningsDto> HandleAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken);
        if (driver == null)
        {
            return new DriverEarningsDto(Guid.Empty, 0m, 0);
        }

        var completedRidesCount = await _context.Rides.CountAsync(r => r.DriverId == driverId && r.Status == Domain.Enums.RideStatus.Completed, cancellationToken);

        return _mapper.Map<DriverEarningsDto>(driver, opts => opts.Items["CompletedRidesCount"] = completedRidesCount);
    }
}
