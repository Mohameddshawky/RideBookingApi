using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Rides.CancelRide;
using RideBookingApi.Application.Features.Rides.EstimateFare;
using RideBookingApi.Application.Features.Rides.GetPassengerRideHistory;
using RideBookingApi.Application.Features.Rides.RequestRide;
using RideBookingApi.Application.Features.Rides.UpdateRideStatus;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PassengersController : ControllerBase
{
    private readonly RequestRideHandler _requestRideHandler;
    private readonly EstimateFareHandler _estimateFareHandler;
    private readonly GetPassengerRideHistoryHandler _getPassengerRideHistoryHandler;
    private readonly CancelRideHandler _cancelRideHandler;
    private readonly UpdateRideStatusHandler _updateRideStatusHandler;

    public PassengersController(
        RequestRideHandler requestRideHandler,
        EstimateFareHandler estimateFareHandler,
        GetPassengerRideHistoryHandler getPassengerRideHistoryHandler,
        CancelRideHandler cancelRideHandler,
        UpdateRideStatusHandler updateRideStatusHandler)
    {
        _requestRideHandler = requestRideHandler;
        _estimateFareHandler = estimateFareHandler;
        _getPassengerRideHistoryHandler = getPassengerRideHistoryHandler;
        _cancelRideHandler = cancelRideHandler;
        _updateRideStatusHandler = updateRideStatusHandler;
    }

    [HttpPost("rides/request")]
    public async Task<ActionResult<RideDto>> RequestRide([FromBody] CreateRideRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var passengerId = await ResolvePassengerIdAsync(userId, cancellationToken);
        if (passengerId is null)
        {
            return BadRequest(new { message = "Passenger profile not found for the authenticated user." });
        }

        var result = await _requestRideHandler.HandleAsync(dto with { }, cancellationToken, passengerId.Value);
        return Ok(result);
    }

    [HttpPost("rides/estimate-fare")]
    public async Task<ActionResult<FareEstimateResponseDto>> EstimateFare([FromBody] EstimateFareRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _estimateFareHandler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("rides/history")]
    public async Task<ActionResult<PassengerRideHistoryResponseDto>> GetRideHistory(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var passengerId = await ResolvePassengerIdAsync(userId, cancellationToken);
        if (passengerId is null)
        {
            return BadRequest(new { message = "Passenger profile not found for the authenticated user." });
        }

        var result = await _getPassengerRideHistoryHandler.HandleAsync(passengerId.Value, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("rides/{rideId:guid}")]
    public async Task<IActionResult> CancelRide(Guid rideId, [FromBody] CancelRideCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _cancelRideHandler.HandleAsync(command with { RideId = rideId }, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("rides/{rideId:guid}/status")]
    public async Task<ActionResult<UpdateRideStatusResponseDto>> UpdateRideStatus(Guid rideId, [FromBody] UpdateRideStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await _updateRideStatusHandler.HandleAsync(command with { RideId = rideId }, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);
    }

    private async Task<Guid?> ResolvePassengerIdAsync(string userId, CancellationToken cancellationToken)
    {
        var passenger = await _requestRideHandler.GetPassengerProfileByUserIdAsync(userId, cancellationToken);
        return passenger?.Id;
    }
}
