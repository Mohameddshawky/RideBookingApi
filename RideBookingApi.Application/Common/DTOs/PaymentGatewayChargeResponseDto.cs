namespace RideBookingApi.Application.Common.DTOs;

public record PaymentGatewayChargeResponseDto(bool IsSuccess, string? TransactionId, string? ErrorMessage);
