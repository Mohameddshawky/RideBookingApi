using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Interfaces.Repositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<Payment?> GetByRideIdAsync(Guid rideId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByPassengerIdAsync(Guid passengerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
}
