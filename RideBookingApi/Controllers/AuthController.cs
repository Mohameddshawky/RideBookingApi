using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Application.Features.Auth.RegisterDriver;
using RideBookingApi.Application.Features.Auth.RegisterPassenger;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost("register/passenger")]
    public async Task<IActionResult> RegisterPassenger([FromBody] PassengerRegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterUserAsync(
            dto.Email,
            dto.Password,
            dto.FirstName,
            dto.LastName,
            UserRoleType.Passenger,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return BadRequest(new { message = "Passenger registration failed." });
        }

        return Ok(result);
    }

    [HttpPost("register/driver")]
    public async Task<IActionResult> RegisterDriver([FromBody] DriverRegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterUserAsync(
            dto.Email,
            dto.Password,
            dto.FirstName,
            dto.LastName,
            UserRoleType.Driver,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return BadRequest(new { message = "Driver registration failed." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(dto.Email, dto.Password, cancellationToken);

        if (string.IsNullOrWhiteSpace(result.Token))
        {
            return Unauthorized();
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        if (string.IsNullOrWhiteSpace(result.Token))
        {
            return Unauthorized();
        }

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        });
    }
}

public record RefreshTokenRequest(string Token, string RefreshToken);
