using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    private readonly RegisterPassengerHandler _registerPassengerHandler;
    private readonly RegisterDriverHandler _registerDriverHandler;

    public AuthController(
        IIdentityService identityService,
        RegisterPassengerHandler registerPassengerHandler,
        RegisterDriverHandler registerDriverHandler)
    {
        _identityService = identityService;
        _registerPassengerHandler = registerPassengerHandler;
        _registerDriverHandler = registerDriverHandler;
    }

    [HttpPost("register/passenger")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RegisterPassenger([FromBody] PassengerRegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _registerPassengerHandler.HandleAsync(dto, cancellationToken);

        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return BadRequest(new { message = "Passenger registration failed." });
        }

        return Ok(result);
    }

    [HttpPost("register/driver")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RegisterDriver([FromBody] DriverRegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _registerDriverHandler.HandleAsync(dto, cancellationToken);

        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return BadRequest(new { message = "Driver registration failed." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var result = await _identityService.LoginAsync(dto.Email.Trim(), dto.Password, cancellationToken);

        if (string.IsNullOrWhiteSpace(result.Token))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Token and refresh token are required." });
        }

        var result = await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken, cancellationToken);

        if (string.IsNullOrWhiteSpace(result.Token))
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
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
