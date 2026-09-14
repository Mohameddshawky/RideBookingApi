using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Features.Rides.RequestRide;

public class RequestRideHandler
{
    private readonly IRideRepository _rideRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFareCalculatorService _fareCalculator;
    private readonly IMapper _mapper;

    public RequestRideHandler(IRideRepository rideRepository, IUnitOfWork unitOfWork, IFareCalculatorService fareCalculator, IMapper mapper)
    {
        _rideRepository = rideRepository;
        _unitOfWork = unitOfWork;
        _fareCalculator = fareCalculator;
        _mapper = mapper;
    }

    public async Task<RideDto> HandleAsync(CreateRideRequestDto dto, CancellationToken cancellationToken = default)
    {
        var estimatedPrice = _fareCalculator.CalculateFare(dto.PickupLatitude, dto.PickupLongitude, dto.DestinationLatitude, dto.DestinationLongitude);

        var ride = _mapper.Map<Ride>(dto);
        ride.EstimatedPrice = estimatedPrice;

        await _rideRepository.AddAsync(ride, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RideDto>(ride);
    }
}
