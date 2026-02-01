# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8002
ENV ASPNETCORE_URLS=http://+:8002

# Health check (shared for final/debug)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8002/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Central package versions (required for restore)
COPY ["Directory.Packages.props", "./"]

# Copy BuildingBlocks
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

# Copy UserProfile service projects
COPY ["Services/UserProfile/UserProfile.TimeCafe.API/UserProfile.TimeCafe.API.csproj", "Services/UserProfile/UserProfile.TimeCafe.API/"]
COPY ["Services/UserProfile/UserProfile.TimeCafe.Application/UserProfile.TimeCafe.Application.csproj", "Services/UserProfile/UserProfile.TimeCafe.Application/"]
COPY ["Services/UserProfile/UserProfile.TimeCafe.Domain/UserProfile.TimeCafe.Domain.csproj", "Services/UserProfile/UserProfile.TimeCafe.Domain/"]
COPY ["Services/UserProfile/UserProfile.TimeCafe.Infrastructure/UserProfile.TimeCafe.Infrastructure.csproj", "Services/UserProfile/UserProfile.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet restore "Services/UserProfile/UserProfile.TimeCafe.API/UserProfile.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/UserProfile/", "Services/UserProfile/"]

WORKDIR "/src/Services/UserProfile/UserProfile.TimeCafe.API"

FROM build AS publish
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "UserProfile.TimeCafe.API.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM build AS publish-debug
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "UserProfile.TimeCafe.API.csproj" -c Debug --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "UserProfile.TimeCafe.API.dll"]

FROM base AS debug
WORKDIR /app
COPY --from=publish-debug /app/publish .

HEALTHCHECK NONE

ENTRYPOINT ["dotnet", "UserProfile.TimeCafe.API.dll"]
