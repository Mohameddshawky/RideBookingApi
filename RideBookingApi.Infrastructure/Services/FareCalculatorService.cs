using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Infrastructure.Services;

public class FareCalculatorService : IFareCalculatorService
{
    public decimal CalculateFare(double pickupLat, double pickupLng, double destLat, double destLng)
    {
        const double earthRadiusKm = 6371.0;

        var latDistance = ToRadians(destLat - pickupLat);
        var lonDistance = ToRadians(destLng - pickupLng);

        var a = Math.Sin(latDistance / 2) * Math.Sin(latDistance / 2)
            + Math.Cos(ToRadians(pickupLat)) * Math.Cos(ToRadians(destLat))
            * Math.Sin(lonDistance / 2) * Math.Sin(lonDistance / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceKm = earthRadiusKm * c;

        var baseFare = 4.50m;
        var perKmFare = 1.75m;
        var total = baseFare + (decimal)distanceKm * perKmFare;

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    private static double ToRadians(double value)
    {
        return value * Math.PI / 180.0;
    }
}
