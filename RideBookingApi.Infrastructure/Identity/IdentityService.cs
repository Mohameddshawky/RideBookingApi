using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;

namespace RideBookingApi.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtSettings = new JwtSettings
        {
            Key = configuration["JwtSettings:Key"] ?? "ThisIsABigSecretKeyForJwtAuthentication1234567890",
            Issuer = configuration["JwtSettings:Issuer"] ?? "RideBookingApi",
            Audience = configuration["JwtSettings:Audience"] ?? "RideBookingApiUsers",
            ExpiryMinutes = int.TryParse(configuration["JwtSettings:ExpiryMinutes"], out var minutes) ? minutes : 60
        };
    }

    public async Task<AuthResponseDto> RegisterUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        UserRoleType role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResponseDto(string.Empty, email, string.Empty, string.Empty, role.ToString());
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return new AuthResponseDto(string.Empty, email, string.Empty, string.Empty, role.ToString());
        }

        var roleName = role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = roleName, Description = $"{roleName} role" });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            UserRoleType = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return new AuthResponseDto(string.Empty, email, string.Empty, string.Empty, roleName);
        }

        await _userManager.AddToRoleAsync(user, roleName);

        return await GenerateAuthResponseAsync(user, roleName);
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    public async Task<AuthResponseDto> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResponseDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        var normalizedEmail = email.Trim();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return new AuthResponseDto(string.Empty, normalizedEmail, string.Empty, string.Empty, string.Empty);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? user.UserRoleType.ToString();

        return await GenerateAuthResponseAsync(user, roleName);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return new AuthResponseDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        var principal = GetPrincipalFromToken(token);
        if (principal is null)
        {
            return new AuthResponseDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("userId");

        if (string.IsNullOrWhiteSpace(userId))
        {
            return new AuthResponseDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResponseDto(string.Empty, user?.Email ?? string.Empty, string.Empty, string.Empty, user?.UserRoleType.ToString() ?? string.Empty);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? user.UserRoleType.ToString();

        return await GenerateAuthResponseAsync(user, roleName);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user, string roleName)
    {
        var token = GenerateJwtToken(user, roleName);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return new AuthResponseDto(
            user.Id,
            user.Email ?? string.Empty,
            token,
            refreshToken,
            roleName);
    }

    private string GenerateJwtToken(ApplicationUser user, string roleName)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("role", roleName),
            new Claim("userId", user.Id)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(_jwtSettings.Issuer),
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(_jwtSettings.Audience),
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
