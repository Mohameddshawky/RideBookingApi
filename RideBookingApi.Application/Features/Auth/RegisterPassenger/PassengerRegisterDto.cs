namespace RideBookingApi.Application.Features.Auth.RegisterPassenger;

public record PassengerRegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);
