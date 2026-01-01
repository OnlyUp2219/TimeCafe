FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8004
ENV ASPNETCORE_URLS=http://+:8004

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy BuildingBlocks
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

# Copy Billing service projects
COPY ["Services/Billing/Billing.TimeCafe.API/Billing.TimeCafe.API.csproj", "Services/Billing/Billing.TimeCafe.API/"]
COPY ["Services/Billing/Billing.TimeCafe.Application/Billing.TimeCafe.Application.csproj", "Services/Billing/Billing.TimeCafe.Application/"]
COPY ["Services/Billing/Billing.TimeCafe.Domain/Billing.TimeCafe.Domain.csproj", "Services/Billing/Billing.TimeCafe.Domain/"]
COPY ["Services/Billing/Billing.TimeCafe.Infrastructure/Billing.TimeCafe.Infrastructure.csproj", "Services/Billing/Billing.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Services/Billing/Billing.TimeCafe.API/Billing.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/Billing/", "Services/Billing/"]

# Build
WORKDIR "/src/Services/Billing/Billing.TimeCafe.API"
RUN dotnet build "Billing.TimeCafe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Billing.TimeCafe.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8004/health || exit 1

ENTRYPOINT ["dotnet", "Billing.TimeCafe.API.dll"]
