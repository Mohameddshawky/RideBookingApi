using RideBookingApi.Application.Common.DTOs;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces;

public interface IPaymentGatewayService
{
    PaymentMethodType SupportedPaymentMethod { get; }
    Task<PaymentGatewayChargeResponseDto> ChargeAsync(decimal amount, string currency, Guid rideId, CancellationToken cancellationToken = default);
}
