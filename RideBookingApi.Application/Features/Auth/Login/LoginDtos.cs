namespace RideBookingApi.Application.Features.Auth.Login;

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(string UserId, string Email, string Token, string RefreshToken, string Role);
