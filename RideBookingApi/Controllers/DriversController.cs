using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Drivers.GetPendingRideRequests;
using RideBookingApi.Application.Features.Drivers.UpdateLocation;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly GetPendingRideRequestsHandler _getPendingRideRequestsHandler;
    private readonly UpdateDriverLocationHandler _updateDriverLocationHandler;

    public DriversController(
        GetPendingRideRequestsHandler getPendingRideRequestsHandler,
        UpdateDriverLocationHandler updateDriverLocationHandler)
    {
        _getPendingRideRequestsHandler = getPendingRideRequestsHandler;
        _updateDriverLocationHandler = updateDriverLocationHandler;
    }

    [HttpGet("rides/pending")]
    public async Task<ActionResult<List<PendingRideRequestDto>>> GetPendingRideRequests(CancellationToken cancellationToken)
    {
        var rides = await _getPendingRideRequestsHandler.HandleAsync(cancellationToken);
        return Ok(rides);
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

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);
    }
}
