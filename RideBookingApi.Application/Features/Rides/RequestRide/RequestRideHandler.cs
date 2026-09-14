using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;

namespace RideBookingApi.Application.Features.Rides.RequestRide;

public class RequestRideHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFareCalculatorService _fareCalculator;
    private readonly IMapper _mapper;

    public RequestRideHandler(IUnitOfWork unitOfWork, IFareCalculatorService fareCalculator, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _fareCalculator = fareCalculator;
        _mapper = mapper;
    }

    public async Task<RideDto> HandleAsync(CreateRideRequestDto dto, CancellationToken cancellationToken = default, Guid? passengerId = null)
    {
        var resolvedPassengerId = passengerId ?? Guid.Empty;
        var passenger = await _unitOfWork.Passengers.GetByIdAsync(resolvedPassengerId, cancellationToken);
        if (passenger is null)
        {
            throw new InvalidOperationException("Passenger profile not found for the authenticated user.");
        }

        var estimatedPrice = _fareCalculator.CalculateFare(dto.PickupLatitude, dto.PickupLongitude, dto.DestinationLatitude, dto.DestinationLongitude);

        var ride = _mapper.Map<Ride>(dto);
        ride.PassengerId = passenger.Id;
        ride.EstimatedPrice = estimatedPrice;

        await _unitOfWork.Rides.AddAsync(ride, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RideDto>(ride);
    }

    public async Task<Passenger?> GetPassengerProfileByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Passengers.GetByUserIdAsync(userId, cancellationToken);
    }
}
