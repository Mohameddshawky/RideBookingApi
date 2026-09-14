using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Admin.GetPlatformStats;
using RideBookingApi.Application.Features.Admin.GetSystemHealth;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly GetPlatformStatsHandler _getPlatformStatsHandler;
    private readonly GetSystemHealthHandler _getSystemHealthHandler;

    public AdminController(
        GetPlatformStatsHandler getPlatformStatsHandler,
        GetSystemHealthHandler getSystemHealthHandler)
    {
        _getPlatformStatsHandler = getPlatformStatsHandler;
        _getSystemHealthHandler = getSystemHealthHandler;
    }

    [HttpGet("platform-stats")]
    public async Task<ActionResult<PlatformStatsDto>> GetPlatformStats(CancellationToken cancellationToken)
    {
        var result = await _getPlatformStatsHandler.HandleAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("system-health")]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealth(CancellationToken cancellationToken)
    {
        var result = await _getSystemHealthHandler.HandleAsync(cancellationToken);
        return Ok(result);
    }
}
