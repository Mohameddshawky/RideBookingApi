using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Application.Features.Rides.EstimateFare;

public class EstimateFareHandler
{
    private readonly IFareCalculatorService _fareCalculatorService;

    public EstimateFareHandler(IFareCalculatorService fareCalculatorService)
    {
        _fareCalculatorService = fareCalculatorService;
    }

    public Task<FareEstimateResponseDto> HandleAsync(EstimateFareRequestDto dto, CancellationToken cancellationToken = default)
    {
        var price = _fareCalculatorService.CalculateFare(dto.PickupLatitude, dto.PickupLongitude, dto.DestinationLatitude, dto.DestinationLongitude);
        var response = new FareEstimateResponseDto(price);
        return Task.FromResult(response);
    }
}
