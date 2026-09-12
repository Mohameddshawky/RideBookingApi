using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Rides.RequestRide;

public class RequestRideHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IFareCalculatorService _fareCalculator;
    private readonly IMapper _mapper;

    public RequestRideHandler(IApplicationDbContext context, IFareCalculatorService fareCalculator, IMapper mapper)
    {
        _context = context;
        _fareCalculator = fareCalculator;
        _mapper = mapper;
    }

    public async Task<RideDto> HandleAsync(CreateRideRequestDto dto, CancellationToken cancellationToken = default)
    {
        var estimatedPrice = _fareCalculator.CalculateFare(dto.PickupLatitude, dto.PickupLongitude, dto.DestinationLatitude, dto.DestinationLongitude);

        var ride = _mapper.Map<Ride>(dto);
        ride.EstimatedPrice = estimatedPrice;

        _context.Rides.Add(ride);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RideDto>(ride);
    }
}
