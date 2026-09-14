using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Payments.ProcessPayment;

public record ProcessPaymentCommand(Guid RideId, PaymentMethodType PaymentMethod);

public record PaymentResultDto(Guid PaymentId, Guid RideId, decimal Amount, PaymentStatus Status, string? TransactionId, string? ErrorMessage);

public class ProcessPaymentHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IPaymentGatewayService> _paymentGateways;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public ProcessPaymentHandler(
        IUnitOfWork unitOfWork,
        IEnumerable<IPaymentGatewayService> paymentGateways,
        INotificationService notificationService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _paymentGateways = paymentGateways;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<PaymentResultDto> HandleAsync(ProcessPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var ride = await _unitOfWork.Rides.GetByIdAsync(command.RideId, cancellationToken);

        if (ride == null)
        {
            return new PaymentResultDto(Guid.Empty, command.RideId, 0m, PaymentStatus.Failed, null, "Ride not found.");
        }

        if (ride.PaymentStatus == PaymentStatus.Paid)
        {
            return new PaymentResultDto(Guid.Empty, ride.Id, ride.FinalPrice ?? ride.EstimatedPrice, PaymentStatus.Failed, null, "Ride is already paid.");
        }

        var amount = ride.FinalPrice ?? ride.EstimatedPrice;

        var gateway = _paymentGateways.FirstOrDefault(g => g.SupportedPaymentMethod == command.PaymentMethod);
        if (gateway == null)
        {
            return new PaymentResultDto(Guid.Empty, ride.Id, amount, PaymentStatus.Failed, null, $"Payment provider for '{command.PaymentMethod}' is not configured.");
        }

        var gatewayResult = await gateway.ChargeAsync(amount, "USD", ride.Id, cancellationToken);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            RideId = ride.Id,
            PassengerId = ride.PassengerId,
            Amount = amount,
            PaymentMethod = command.PaymentMethod,
            ProcessedAt = DateTime.UtcNow
        };

        if (gatewayResult.IsSuccess)
        {
            payment.Status = PaymentStatus.Paid;
            payment.TransactionId = gatewayResult.TransactionId;
            ride.PaymentStatus = PaymentStatus.Paid;
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = gatewayResult.ErrorMessage;
            ride.PaymentStatus = PaymentStatus.Failed;
        }

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var passenger = await _unitOfWork.Passengers.GetByIdAsync(ride.PassengerId, cancellationToken);
        if (passenger != null)
        {
            var notificationTitle = gatewayResult.IsSuccess ? "Payment Successful" : "Payment Failed";
            var notificationMsg = gatewayResult.IsSuccess 
                ? $"Your payment of ${amount} for ride {ride.Id} succeeded."
                : $"Your payment of ${amount} failed: {gatewayResult.ErrorMessage}";

            await _notificationService.SendNotificationAsync(
                passenger.UserId,
                gatewayResult.IsSuccess ? NotificationType.PaymentSuccessful : NotificationType.PaymentFailed,
                notificationTitle,
                notificationMsg,
                cancellationToken);
        }

        return _mapper.Map<PaymentResultDto>(payment);
    }
}
