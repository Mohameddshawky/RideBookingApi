using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Drivers.AcceptRide;
using RideBookingApi.Application.Features.Drivers.GetEarnings;
using RideBookingApi.Application.Features.Drivers.GetPendingRideRequests;
using RideBookingApi.Application.Features.Drivers.ToggleAvailability;
using RideBookingApi.Application.Features.Drivers.UpdateLocation;
using RideBookingApi.Application.Features.Drivers.UploadDocument;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly GetPendingRideRequestsHandler _getPendingRideRequestsHandler;
    private readonly UpdateDriverLocationHandler _updateDriverLocationHandler;
    private readonly AcceptRideHandler _acceptRideHandler;
    private readonly ToggleAvailabilityHandler _toggleAvailabilityHandler;
    private readonly UploadDocumentHandler _uploadDocumentHandler;
    private readonly GetEarningsHandler _getEarningsHandler;

    public DriversController(
        GetPendingRideRequestsHandler getPendingRideRequestsHandler,
        UpdateDriverLocationHandler updateDriverLocationHandler,
        AcceptRideHandler acceptRideHandler,
        ToggleAvailabilityHandler toggleAvailabilityHandler,
        UploadDocumentHandler uploadDocumentHandler,
        GetEarningsHandler getEarningsHandler)
    {
        _getPendingRideRequestsHandler = getPendingRideRequestsHandler;
        _updateDriverLocationHandler = updateDriverLocationHandler;
        _acceptRideHandler = acceptRideHandler;
        _toggleAvailabilityHandler = toggleAvailabilityHandler;
        _uploadDocumentHandler = uploadDocumentHandler;
        _getEarningsHandler = getEarningsHandler;
    }

    [HttpGet("rides/pending")]
    public async Task<ActionResult<List<PendingRideRequestDto>>> GetPendingRideRequests(CancellationToken cancellationToken)
    {
        var rides = await _getPendingRideRequestsHandler.HandleAsync(cancellationToken);
        return Ok(rides);
    }

    [HttpPost("rides/accept")]
    public async Task<ActionResult<AcceptRideResponseDto>> AcceptRide([FromBody] AcceptRideCommand command, CancellationToken cancellationToken)
    {
        var result = await _acceptRideHandler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("availability")]
    public async Task<ActionResult<ToggleAvailabilityResponseDto>> ToggleAvailability([FromBody] ToggleAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var result = await _toggleAvailabilityHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateDriverLocationCommand command, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var updated = await _updateDriverLocationHandler.HandleAsync(userId, command, cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("documents/upload")]
    public async Task<ActionResult<UploadDocumentResponseDto>> UploadDocument([FromBody] UploadDocumentCommand command, CancellationToken cancellationToken)
    {
        var result = await _uploadDocumentHandler.HandleAsync(command, cancellationToken);
        if (result.DocumentId == Guid.Empty)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{driverId:guid}/earnings")]
    public async Task<ActionResult<DriverEarningsDto>> GetEarnings(Guid driverId, CancellationToken cancellationToken)
    {
        var result = await _getEarningsHandler.HandleAsync(driverId, cancellationToken);
        if (result.DriverId == Guid.Empty)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);
    }
}
