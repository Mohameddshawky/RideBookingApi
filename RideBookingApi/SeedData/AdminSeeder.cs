using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RideBookingApi.Domain.Constants;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Domain.Enums;
using RideBookingApi.Infrastructure.Persistence;

namespace RideBookingApi.SeedData;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("SeedAdmin");
        var email = settings["Email"];
        var password = settings["Password"];

        // Do not create a predictable account unless credentials were deliberately configured.
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        if (!await roleManager.RoleExistsAsync(UserRoles.Administrator))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Name = UserRoles.Administrator,
                Description = "Administrator role"
            });

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create the {UserRoles.Administrator} role: {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
            }
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = settings["FirstName"] ?? "System",
                LastName = settings["LastName"] ?? "Administrator",
                UserRoleType = UserRoleType.Administrator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await userManager.CreateAsync(user, password);
            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to create the seeded administrator: {string.Join(", ", userResult.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, UserRoles.Administrator))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, UserRoles.Administrator);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Unable to assign the administrator role: {string.Join(", ", addRoleResult.Errors.Select(error => error.Description))}");
            }
        }

        if (user.UserRoleType != UserRoleType.Administrator)
        {
            user.UserRoleType = UserRoleType.Administrator;
            await userManager.UpdateAsync(user);
        }

        if (!await dbContext.Administrators.AnyAsync(administrator => administrator.UserId == user.Id))
        {
            dbContext.Administrators.Add(new Administrator
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Department = settings["Department"] ?? "Platform Administration"
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
