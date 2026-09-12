using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResponseDto> RegisterUserAsync(
        string email, string password, string firstName, string lastName, UserRoleType role, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default);

    Task<AuthResponseDto> RefreshTokenAsync(
        string token, string refreshToken, CancellationToken cancellationToken = default);
}
