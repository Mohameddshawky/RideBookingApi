using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Domain.Entities;

public class Ride
{
    public Guid Id { get; set; }

    public Guid PassengerId { get; set; }
    public Passenger Passenger { get; set; } = null!;

    public Guid? DriverId { get; set; }
    public Driver? Driver { get; set; }

    public string PickupAddress { get; set; } = string.Empty;
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }

    public string DestinationAddress { get; set; } = string.Empty;
    public double DestinationLatitude { get; set; }
    public double DestinationLongitude { get; set; }

    public decimal EstimatedPrice { get; set; }
    public decimal? FinalPrice { get; set; }

    public RideStatus Status { get; set; } = RideStatus.Requested;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AssignedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public Payment? Payment { get; set; }
}
