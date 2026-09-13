using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Features.Notifications;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;
using RideBookingApi.Tests.Infrastructure;
using Xunit;

namespace RideBookingApi.Tests.Notification;

public class NotificationHandlersTests
{
    private static IApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TestApplicationDbContext(options);

        var userA = new ApplicationUser { Id = "user-1", UserName = "user1@test.com", Email = "user1@test.com" };
        var userB = new ApplicationUser { Id = "user-2", UserName = "user2@test.com", Email = "user2@test.com" };

        context.Users.AddRange(userA, userB);
        context.Notifications.AddRange(
            new RideBookingApi.Domain.Entities.Notification { Id = Guid.NewGuid(), UserId = "user-1", Title = "Hello", Message = "World", Type = NotificationType.SystemMessage, IsRead = true },
            new RideBookingApi.Domain.Entities.Notification { Id = Guid.NewGuid(), UserId = "user-1", Title = "Unread", Message = "Test", Type = NotificationType.RideStatusUpdate, IsRead = false },
            new RideBookingApi.Domain.Entities.Notification { Id = Guid.NewGuid(), UserId = "user-2", Title = "Other", Message = "Hidden", Type = NotificationType.PaymentStatusUpdate, IsRead = false }
        );

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetNotifications_ReturnsOnlyCurrentUsersNotifications()
    {
        var context = CreateContext();
        var handler = new GetNotificationsHandler(context);

        var result = await handler.HandleAsync("user-1");

        Assert.Equal(2, result.Count);
        Assert.All(result, x => Assert.Equal("user-1", x.UserId));
    }

    [Fact]
    public async Task GetUnreadNotifications_ReturnsUnreadOnly()
    {
        var context = CreateContext();
        var handler = new GetUnreadNotificationsHandler(context);

        var result = await handler.HandleAsync("user-1");

        Assert.Single(result);
        Assert.All(result, x => Assert.False(x.IsRead));
    }

    [Fact]
    public async Task MarkNotificationAsRead_OnlyMarksOwnedNotification()
    {
        var context = CreateContext();
        var handler = new MarkNotificationAsReadHandler(context);

        var all = await new GetNotificationsHandler(context).HandleAsync("user-1");
        var target = all.First();

        var updated = await handler.HandleAsync("user-1", target.Id);

        Assert.True(updated);
        var refreshed = await new GetNotificationsHandler(context).HandleAsync("user-1");
        Assert.Contains(refreshed, x => x.Id == target.Id && x.IsRead);
    }

    [Fact]
    public async Task MarkAllNotificationsAsRead_OnlyMarksCurrentUsersNotifications()
    {
        var context = CreateContext();
        var handler = new MarkAllNotificationsAsReadHandler(context);

        var updatedCount = await handler.HandleAsync("user-1");

        Assert.Equal(1, updatedCount);
        var remaining = await new GetUnreadNotificationsHandler(context).HandleAsync("user-1");
        Assert.Empty(remaining);
    }
}
