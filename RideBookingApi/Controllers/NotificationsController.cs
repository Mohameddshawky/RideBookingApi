using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideBookingApi.Application.Features.Notifications;

namespace RideBookingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly GetNotificationsHandler _getNotificationsHandler;
    private readonly GetUnreadNotificationsHandler _getUnreadNotificationsHandler;
    private readonly MarkNotificationAsReadHandler _markNotificationAsReadHandler;
    private readonly MarkAllNotificationsAsReadHandler _markAllNotificationsAsReadHandler;

    public NotificationsController(
        GetNotificationsHandler getNotificationsHandler,
        GetUnreadNotificationsHandler getUnreadNotificationsHandler,
        MarkNotificationAsReadHandler markNotificationAsReadHandler,
        MarkAllNotificationsAsReadHandler markAllNotificationsAsReadHandler)
    {
        _getNotificationsHandler = getNotificationsHandler;
        _getUnreadNotificationsHandler = getUnreadNotificationsHandler;
        _markNotificationAsReadHandler = markNotificationAsReadHandler;
        _markAllNotificationsAsReadHandler = markAllNotificationsAsReadHandler;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetNotifications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var notifications = await _getNotificationsHandler.HandleAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUnreadNotifications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var notifications = await _getUnreadNotificationsHandler.HandleAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var result = await _markNotificationAsReadHandler.HandleAsync(userId, id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<int>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var updatedCount = await _markAllNotificationsAsReadHandler.HandleAsync(userId, cancellationToken);
        return Ok(updatedCount);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);
    }
}
