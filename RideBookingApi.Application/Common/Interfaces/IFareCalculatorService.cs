namespace RideBookingApi.Application.Common.Interfaces;

public interface IFareCalculatorService
{
    decimal CalculateFare(double pickupLat, double pickupLng, double destLat, double destLng);
}
