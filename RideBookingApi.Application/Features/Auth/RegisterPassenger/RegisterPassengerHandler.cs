using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Auth.RegisterPassenger;

public class RegisterPassengerHandler
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterPassengerHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> HandleAsync(PassengerRegisterDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _identityService.RegisterUserAsync(dto.Email, dto.Password, dto.FirstName, dto.LastName, UserRoleType.Passenger, cancellationToken);
        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return new AuthResponseDto(string.Empty, dto.Email, string.Empty, string.Empty, UserRoleType.Passenger.ToString());
        }

        var passenger = _mapper.Map<Passenger>(dto);
        passenger.UserId = result.UserId;

        await _unitOfWork.Passengers.AddAsync(passenger, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
