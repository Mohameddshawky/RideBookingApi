FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RideBookingApi/RideBookingApi.csproj", "RideBookingApi/"]
COPY ["RideBookingApi.Domain/RideBookingApi.Domain.csproj", "RideBookingApi.Domain/"]
COPY ["RideBookingApi.Application/RideBookingApi.Application.csproj", "RideBookingApi.Application/"]
COPY ["RideBookingApi.Infrastructure/RideBookingApi.Infrastructure.csproj", "RideBookingApi.Infrastructure/"]

RUN dotnet restore "./RideBookingApi/RideBookingApi.csproj"
COPY . .
WORKDIR "/src/RideBookingApi"
RUN dotnet build "./RideBookingApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RideBookingApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RideBookingApi.dll"]
