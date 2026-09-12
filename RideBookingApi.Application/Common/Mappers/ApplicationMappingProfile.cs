using AutoMapper;
using RideBookingApi.Application.Features.Auth.RegisterDriver;
using RideBookingApi.Application.Features.Auth.RegisterPassenger;
using RideBookingApi.Application.Features.Drivers.GetEarnings;
using RideBookingApi.Application.Features.Drivers.UploadDocument;
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

        CreateMap<Driver, DriverEarningsDto>()
            .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TotalEarnings, opt => opt.MapFrom(src => src.TotalEarnings))
            .ForMember(dest => dest.CompletedRidesCount, opt => opt.MapFrom((src, _, _, context) => context.Items.ContainsKey("CompletedRidesCount") ? context.Items["CompletedRidesCount"] : 0));

        CreateMap<DriverDocument, UploadDocumentResponseDto>()
            .ForMember(dest => dest.DocumentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<Payment, PaymentResultDto>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RideId, opt => opt.MapFrom(src => src.RideId))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
            .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.ErrorMessage));
    }
}
