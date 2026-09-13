using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Mappers;
using RideBookingApi.Application.Common.Services;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Infrastructure.Identity;
using RideBookingApi.Infrastructure.Notifications;
using RideBookingApi.Infrastructure.Persistence;
using RideBookingApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(ApplicationMappingProfile));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("RideBookingDb"));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRideLifecycleService, RideLifecycleService>();
builder.Services.AddScoped<IDriverMatchingService, DriverMatchingService>();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IUserIdProvider, JwtUserIdProvider>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapControllers();

app.Run();
