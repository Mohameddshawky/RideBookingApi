using RideBookingApi.Application.Common.Interfaces;

namespace RideBookingApi.Application.Features.Auth.Login;

public class LoginHandler
{
    private readonly IIdentityService _identityService;

    public LoginHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResponseDto> HandleAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        return await _identityService.LoginAsync(dto.Email, dto.Password, cancellationToken);
    }
}
