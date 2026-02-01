# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8003
ENV ASPNETCORE_URLS=http://+:8003

# Health check (shared for final/debug)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8003/health || exit 1

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
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet restore "Services/Venue/Venue.TimeCafe.API/Venue.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/Venue/", "Services/Venue/"]

WORKDIR "/src/Services/Venue/Venue.TimeCafe.API"

FROM build AS publish
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "Venue.TimeCafe.API.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM build AS publish-debug
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "Venue.TimeCafe.API.csproj" -c Debug --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Venue.TimeCafe.API.dll"]

FROM base AS debug
WORKDIR /app
COPY --from=publish-debug /app/publish .

HEALTHCHECK NONE

ENTRYPOINT ["dotnet", "Venue.TimeCafe.API.dll"]
