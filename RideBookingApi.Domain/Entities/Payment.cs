using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }

    public Guid RideId { get; set; }
    public Ride Ride { get; set; } = null!;

    public Guid PassengerId { get; set; }
    public Passenger Passenger { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
