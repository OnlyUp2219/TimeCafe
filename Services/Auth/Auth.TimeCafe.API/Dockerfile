# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8001
ENV ASPNETCORE_URLS=http://+:8001

# Health check (shared for final/debug)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8001/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Central package versions (required for restore)
COPY ["Directory.Packages.props", "./"]

# Copy BuildingBlocks
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

# Copy Auth service projects
COPY ["Services/Auth/Auth.TimeCafe.API/Auth.TimeCafe.API.csproj", "Services/Auth/Auth.TimeCafe.API/"]
COPY ["Services/Auth/Auth.TimeCafe.Application/Auth.TimeCafe.Application.csproj", "Services/Auth/Auth.TimeCafe.Application/"]
COPY ["Services/Auth/Auth.TimeCafe.Domain/Auth.TimeCafe.Domain.csproj", "Services/Auth/Auth.TimeCafe.Domain/"]
COPY ["Services/Auth/Auth.TimeCafe.Infrastructure/Auth.TimeCafe.Infrastructure.csproj", "Services/Auth/Auth.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet \
    dotnet restore "Services/Auth/Auth.TimeCafe.API/Auth.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/Auth/", "Services/Auth/"]

WORKDIR "/src/Services/Auth/Auth.TimeCafe.API"

FROM build AS publish
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet \
    dotnet publish "Auth.TimeCafe.API.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM build AS publish-debug
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet \
    dotnet publish "Auth.TimeCafe.API.csproj" -c Debug --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Auth.TimeCafe.API.dll"]

FROM base AS debug
WORKDIR /app
COPY --from=publish-debug /app/publish .

ENTRYPOINT ["dotnet", "Auth.TimeCafe.API.dll"]
