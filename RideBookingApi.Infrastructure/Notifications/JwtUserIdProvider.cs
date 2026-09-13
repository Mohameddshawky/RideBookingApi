using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace RideBookingApi.Infrastructure.Notifications;

public class JwtUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue("userId")
            ?? connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? connection.User?.FindFirstValue(ClaimTypes.Name);
    }
}
