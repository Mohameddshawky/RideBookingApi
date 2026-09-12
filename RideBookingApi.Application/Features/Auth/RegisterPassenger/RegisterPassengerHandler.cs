using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Auth.RegisterPassenger;

public class RegisterPassengerHandler
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RegisterPassengerHandler(IIdentityService identityService, IApplicationDbContext context, IMapper mapper)
    {
        _identityService = identityService;
        _context = context;
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

        _context.Passengers.Add(passenger);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
