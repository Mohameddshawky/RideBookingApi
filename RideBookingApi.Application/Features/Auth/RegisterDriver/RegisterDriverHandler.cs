using AutoMapper;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Application.Features.Auth.RegisterDriver;

public class RegisterDriverHandler
{
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterDriverHandler(IIdentityService identityService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> HandleAsync(DriverRegisterDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _identityService.RegisterUserAsync(dto.Email, dto.Password, dto.FirstName, dto.LastName, UserRoleType.Driver, cancellationToken);
        if (string.IsNullOrWhiteSpace(result.UserId))
        {
            return new AuthResponseDto(string.Empty, dto.Email, string.Empty, string.Empty, UserRoleType.Driver.ToString());
        }

        try
        {
            var driver = _mapper.Map<Driver>(dto);
            driver.UserId = result.UserId;

            await _unitOfWork.Drivers.AddAsync(driver, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _identityService.DeleteUserAsync(result.UserId);
            throw;
        }

        return result;
    }
}
