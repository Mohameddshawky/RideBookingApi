using Microsoft.EntityFrameworkCore;
using RideBookingApi.Application.Common.Services;
using RideBookingApi.Application.Features.Rides.GetPassengerRideHistory;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;
using RideBookingApi.Tests.Infrastructure;
using Xunit;

namespace RideBookingApi.Tests;

public class RideLifecycleServiceTests
{
    [Fact]
    public void CanTransition_Requested_To_DriverAssigned_IsAllowed()
    {
        var service = new RideLifecycleService();

        var result = service.CanTransition(RideStatus.Requested, RideStatus.DriverAssigned);

        Assert.True(result);
    }

    [Fact]
    public void CanTransition_Requested_To_Completed_IsNotAllowed()
    {
        var service = new RideLifecycleService();

        var result = service.CanTransition(RideStatus.Requested, RideStatus.Completed);

        Assert.False(result);
    }

    [Fact]
    public async Task GetPassengerRideHistoryHandler_Returns_Rides_OrderedByMostRecent()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestApplicationDbContext(options);

        var passenger = new Passenger { Id = Guid.NewGuid(), UserId = "user-1" };
        context.Passengers.Add(passenger);

        var olderRide = new Ride
        {
            Id = Guid.NewGuid(),
            PassengerId = passenger.Id,
            Passenger = passenger,
            PickupAddress = "Old pickup",
            DestinationAddress = "Old destination",
            PickupLatitude = 1,
            PickupLongitude = 1,
            DestinationLatitude = 2,
            DestinationLongitude = 2,
            EstimatedPrice = 50m,
            RequestedAt = DateTime.UtcNow.AddDays(-2),
            Status = RideStatus.Completed
        };

        var latestRide = new Ride
        {
            Id = Guid.NewGuid(),
            PassengerId = passenger.Id,
            Passenger = passenger,
            PickupAddress = "New pickup",
            DestinationAddress = "New destination",
            PickupLatitude = 3,
            PickupLongitude = 3,
            DestinationLatitude = 4,
            DestinationLongitude = 4,
            EstimatedPrice = 90m,
            RequestedAt = DateTime.UtcNow,
            Status = RideStatus.InProgress
        };

        context.Rides.AddRange(olderRide, latestRide);
        await context.SaveChangesAsync();

        var handler = new GetPassengerRideHistoryHandler(context);

        var result = await handler.HandleAsync(passenger.Id);

        Assert.Equal(2, result.Rides.Count);
        Assert.Equal(latestRide.Id, result.Rides[0].Id);
        Assert.Equal(olderRide.Id, result.Rides[1].Id);
    }
}
