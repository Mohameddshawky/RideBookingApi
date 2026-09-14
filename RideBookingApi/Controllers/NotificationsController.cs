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
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _getNotificationsHandler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUnreadNotifications(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _getUnreadNotificationsHandler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _markNotificationAsReadHandler.HandleAsync(userId, notificationId, cancellationToken);
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
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var result = await _markAllNotificationsAsReadHandler.HandleAsync(userId, cancellationToken);
        return Ok(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue("userId")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name);
    }
}
