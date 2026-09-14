using Microsoft.Extensions.Configuration;
using RideBookingApi.Application.Common.DTOs;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Domain.Enums;
using Stripe;

namespace RideBookingApi.Infrastructure.Services;

public class StripePaymentGatewayService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly PaymentIntentService _paymentIntentService;

    public StripePaymentGatewayService(IConfiguration configuration)
    {
        _configuration = configuration;
        _paymentIntentService = new PaymentIntentService();
    }

    public PaymentMethodType SupportedPaymentMethod => PaymentMethodType.CreditCard;

    public async Task<PaymentGatewayChargeResponseDto> ChargeAsync(decimal amount, string currency, Guid rideId, CancellationToken cancellationToken = default)
    {
        if (amount <= 0m)
        {
            return new PaymentGatewayChargeResponseDto(false, null, "Amount must be greater than zero.");
        }

        var apiKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new PaymentGatewayChargeResponseDto(false, null, "Stripe secret key is not configured.");
        }

        StripeConfiguration.ApiKey = apiKey;

        var amountInCents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = currency.ToLowerInvariant(),
            Metadata = new Dictionary<string, string>
            {
                ["rideId"] = rideId.ToString()
            },
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            }
        };

        try
        {
            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            if (paymentIntent.Status == "succeeded")
            {
                return new PaymentGatewayChargeResponseDto(true, paymentIntent.Id, null);
            }

            return new PaymentGatewayChargeResponseDto(false, paymentIntent.Id, $"Stripe payment is not succeeded. Current status: {paymentIntent.Status}");
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayChargeResponseDto(false, null, ex.Message);
        }
    }
}
