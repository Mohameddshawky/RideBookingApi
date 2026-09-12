using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Auth.RegisterDriver;

public class RegisterDriverHandler
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RegisterDriverHandler(IIdentityService identityService, IApplicationDbContext context, IMapper mapper)
    {
        _identityService = identityService;
        _context = context;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> HandleAsync(DriverRegisterDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _identityService.RegisterUserAsync(dto.Email, dto.Password, dto.FirstName, dto.LastName, UserRoleType.Driver, cancellationToken);
        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return new AuthResponseDto(string.Empty, dto.Email, string.Empty, string.Empty, UserRoleType.Driver.ToString());
        }

        var driver = _mapper.Map<Driver>(dto);
        driver.UserId = result.UserId;

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
