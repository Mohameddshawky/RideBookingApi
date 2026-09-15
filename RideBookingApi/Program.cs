using System.Text;
using System.Threading.RateLimiting;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using RideBookingApi.Application.Common.Interfaces;
using RideBookingApi.Application.Common.Interfaces.Repositories;
using RideBookingApi.Application.Common.Mappers;
using RideBookingApi.Application.Common.Services;
using RideBookingApi.Application.Features.Admin.GetPlatformStats;
using RideBookingApi.Application.Features.Admin.GetSystemHealth;
using RideBookingApi.Application.Features.Auth.Login;
using RideBookingApi.Application.Features.Auth.RegisterDriver;
using RideBookingApi.Application.Features.Auth.RegisterPassenger;
using RideBookingApi.Application.Features.Drivers.AcceptRide;
using RideBookingApi.Application.Features.Drivers.GetEarnings;
using RideBookingApi.Application.Features.Drivers.GetPendingRideRequests;
using RideBookingApi.Application.Features.Drivers.ToggleAvailability;
using RideBookingApi.Application.Features.Drivers.UpdateLocation;
using RideBookingApi.Application.Features.Drivers.UploadDocument;
using RideBookingApi.Application.Features.Notifications;
using RideBookingApi.Application.Features.Payments.ProcessPayment;
using RideBookingApi.Application.Features.Rides.CancelRide;
using RideBookingApi.Application.Features.Rides.EstimateFare;
using RideBookingApi.Application.Features.Rides.GetPassengerRideHistory;
using RideBookingApi.Application.Features.Rides.RequestRide;
using RideBookingApi.Application.Features.Rides.UpdateRideStatus;
using RideBookingApi.Domain.Entities;
using RideBookingApi.Infrastructure.Identity;
using RideBookingApi.Infrastructure.Notifications;
using RideBookingApi.Infrastructure.Persistence;
using RideBookingApi.Infrastructure.Persistence.Repositories;
using RideBookingApi.Infrastructure.Services;
using RideBookingApi.SeedData;

var builder = WebApplication.CreateBuilder(args);

var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (!string.IsNullOrWhiteSpace(stripeSecretKey))
{
    StripeConfiguration.ApiKey = stripeSecretKey;
}

builder.Services.AddAutoMapper(typeof(ApplicationMappingProfile));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=RideBookingDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Too many requests. Please slow down and try again later.",
            statusCode = StatusCodes.Status429TooManyRequests
        }, cancellationToken: token);
    };

    options.AddPolicy("AuthPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("PaymentPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

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
builder.Services.AddScoped<IIdentityService, RideBookingApi.Infrastructure.Identity.IdentityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRideLifecycleService, RideLifecycleService>();
builder.Services.AddScoped<IFareCalculatorService, FareCalculatorService>();
builder.Services.AddScoped<IDriverMatchingService, DriverMatchingService>();
builder.Services.AddScoped<IPaymentGatewayService, StripePaymentGatewayService>();
builder.Services.AddScoped<IPaymentGatewayService, CashPaymentGatewayService>();

builder.Services.AddScoped<GetPendingRideRequestsHandler>();
builder.Services.AddScoped<UpdateDriverLocationHandler>();
builder.Services.AddScoped<AcceptRideHandler>();
builder.Services.AddScoped<ToggleAvailabilityHandler>();
builder.Services.AddScoped<UploadDocumentHandler>();
builder.Services.AddScoped<GetEarningsHandler>();
builder.Services.AddScoped<RequestRideHandler>();
builder.Services.AddScoped<EstimateFareHandler>();
builder.Services.AddScoped<CancelRideHandler>();
builder.Services.AddScoped<GetPassengerRideHistoryHandler>();
builder.Services.AddScoped<UpdateRideStatusHandler>();
builder.Services.AddScoped<GetPlatformStatsHandler>();
builder.Services.AddScoped<GetSystemHealthHandler>();
builder.Services.AddScoped<GetNotificationsHandler>();
builder.Services.AddScoped<GetUnreadNotificationsHandler>();
builder.Services.AddScoped<MarkNotificationAsReadHandler>();
builder.Services.AddScoped<MarkAllNotificationsAsReadHandler>();
builder.Services.AddScoped<ProcessPaymentHandler>();

builder.Services.AddScoped<RegisterPassengerHandler>();
builder.Services.AddScoped<RegisterDriverHandler>();
builder.Services.AddScoped<LoginHandler>();

builder.Services.AddControllers();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IDriverDocumentRepository, DriverDocumentRepository>();
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
builder.Services.AddScoped<IRideRepository, RideRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddSignalR();

builder.Services.AddSingleton<IUserIdProvider, JwtUserIdProvider>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RideBookingApi",
        Version = "v1",
        Description = "RideBooking API with JWT authentication"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(exception, "Unhandled exception occurred while processing request: {Path}", context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(new
        {
            message = exception?.Message ?? "An unexpected error occurred.",
            statusCode = context.Response.StatusCode,
            path = context.Request.Path.Value,
            timestamp = DateTime.UtcNow
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RideBookingApi v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapControllers();

app.Run();
