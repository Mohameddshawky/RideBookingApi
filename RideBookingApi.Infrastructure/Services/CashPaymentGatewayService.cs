using RideBookingApi.Application.Common.DTOs;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Services;

public class CashPaymentGatewayService : IPaymentGatewayService
{
    public PaymentMethodType SupportedPaymentMethod => PaymentMethodType.Cash;

    public Task<PaymentGatewayChargeResponseDto> ChargeAsync(decimal amount, string currency, Guid rideId, CancellationToken cancellationToken = default)
    {
        if (amount <= 0m)
        {
            return Task.FromResult(new PaymentGatewayChargeResponseDto(false, null, "Cash amount must be greater than zero."));
        }

        var transactionId = $"cash_{rideId}_{Guid.NewGuid():N}";

        return Task.FromResult(new PaymentGatewayChargeResponseDto(true, transactionId, null));
    }
}
