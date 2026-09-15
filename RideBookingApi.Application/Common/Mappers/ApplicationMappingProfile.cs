using AutoMapper;
using RideBookingApi.Application.Features.Auth.RegisterDriver;
using RideBookingApi.Application.Features.Auth.RegisterPassenger;
using RideBookingApi.Application.Features.Payments.ProcessPayment;
using RideBookingApi.Application.Features.Rides.RequestRide;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Common.Mappers;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<PassengerRegisterDto, Passenger>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationUser, opt => opt.Ignore())
            .ForMember(dest => dest.Rides, opt => opt.Ignore())
            .ForMember(dest => dest.Payments, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(_ => 5.0));

        CreateMap<DriverRegisterDto, Driver>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ApplicationUser, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.Rides, opt => opt.Ignore())
            .ForMember(dest => dest.AvailabilityStatus, opt => opt.MapFrom(_ => DriverAvailabilityStatus.Offline))
            .ForMember(dest => dest.CurrentLatitude, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentLongitude, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(_ => 5.0))
            .ForMember(dest => dest.TotalEarnings, opt => opt.MapFrom(_ => 0m));

        CreateMap<CreateRideRequestDto, Ride>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.DriverId, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedPrice, opt => opt.Ignore())
            .ForMember(dest => dest.FinalPrice, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => RideStatus.Requested))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => PaymentStatus.Pending))
            .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.AssignedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ArrivedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
            .ForMember(dest => dest.Payment, opt => opt.Ignore())
            .ForMember(dest => dest.Passenger, opt => opt.Ignore());

        CreateMap<Ride, RideDto>()
            .ConstructUsing(src => new RideDto(
                src.Id,
                src.PassengerId,
                src.DriverId,
                src.PickupAddress,
                src.DestinationAddress,
                src.EstimatedPrice,
                src.FinalPrice,
                src.Status,
                src.PaymentStatus,
                src.RequestedAt));

        CreateMap<Payment, PaymentResultDto>()
            .ConstructUsing(src => new PaymentResultDto(
                src.Id,
                src.RideId,
                src.Amount,
                src.Status,
                src.TransactionId,
                src.ErrorMessage));
    }
}
