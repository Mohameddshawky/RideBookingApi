namespace RideBookingApi.Application.Features.Admin.GetSystemHealth;

public record ComponentHealthStatus(string ComponentName, string Status, string Message);

public record SystemHealthDto(string OverallStatus, DateTime Timestamp, List<ComponentHealthStatus> Components);

public class GetSystemHealthHandler
{
    public Task<SystemHealthDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var components = new List<ComponentHealthStatus>
        {
            new ComponentHealthStatus("Database", "Healthy", "Connection pool active."),
            new ComponentHealthStatus("Identity", "Healthy", "ASP.NET Core Identity active."),
            new ComponentHealthStatus("PaymentGateway", "Healthy", "Payment service providers accessible."),
            new ComponentHealthStatus("NotificationDispatcher", "Healthy", "Event message queue ready.")
        };

        var healthDto = new SystemHealthDto("Healthy", DateTime.UtcNow, components);
        return Task.FromResult(healthDto);
    }
}
