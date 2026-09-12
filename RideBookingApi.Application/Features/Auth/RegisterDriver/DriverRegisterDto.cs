namespace RideBookingApi.Application.Features.Auth.RegisterDriver;

public record DriverRegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string LicenseNumber,
    string VehicleModel,
    string VehiclePlateNumber,
    string VehicleColor
);
