FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8003
ENV ASPNETCORE_URLS=http://+:8003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Central package versions (required for restore)
COPY ["Directory.Packages.props", "./"]

# Copy BuildingBlocks
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

# Copy Venue service projects
COPY ["Services/Venue/Venue.TimeCafe.API/Venue.TimeCafe.API.csproj", "Services/Venue/Venue.TimeCafe.API/"]
COPY ["Services/Venue/Venue.TimeCafe.Application/Venue.TimeCafe.Application.csproj", "Services/Venue/Venue.TimeCafe.Application/"]
COPY ["Services/Venue/Venue.TimeCafe.Domain/Venue.TimeCafe.Domain.csproj", "Services/Venue/Venue.TimeCafe.Domain/"]
COPY ["Services/Venue/Venue.TimeCafe.Infrastructure/Venue.TimeCafe.Infrastructure.csproj", "Services/Venue/Venue.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Services/Venue/Venue.TimeCafe.API/Venue.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/Venue/", "Services/Venue/"]

# Build
WORKDIR "/src/Services/Venue/Venue.TimeCafe.API"
RUN dotnet build "Venue.TimeCafe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Venue.TimeCafe.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8003/health || exit 1

ENTRYPOINT ["dotnet", "Venue.TimeCafe.API.dll"]
