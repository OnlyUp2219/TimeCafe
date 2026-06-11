# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8005
ENV ASPNETCORE_URLS=http://+:8005

# Health check (shared for final/debug)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8005/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Central package versions (required for restore)
COPY ["Directory.Packages.props", "./"]
COPY ["appsettings.shared.json", "./"]

# Copy BuildingBlocks and ServiceDefaults
COPY ["Services/BuildingBlocks/BuildingBlocks.csproj", "Services/BuildingBlocks/"]
COPY ["Services/TimeCafe.ServiceDefaults/TimeCafe.ServiceDefaults.csproj", "Services/TimeCafe.ServiceDefaults/"]

# Copy Audit service projects
COPY ["Services/Audit/Audit.TimeCafe.API/Audit.TimeCafe.API.csproj", "Services/Audit/Audit.TimeCafe.API/"]
COPY ["Services/Audit/Audit.TimeCafe.Application/Audit.TimeCafe.Application.csproj", "Services/Audit/Audit.TimeCafe.Application/"]
COPY ["Services/Audit/Audit.TimeCafe.Domain/Audit.TimeCafe.Domain.csproj", "Services/Audit/Audit.TimeCafe.Domain/"]
COPY ["Services/Audit/Audit.TimeCafe.Infrastructure/Audit.TimeCafe.Infrastructure.csproj", "Services/Audit/Audit.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet restore "Services/Audit/Audit.TimeCafe.API/Audit.TimeCafe.API.csproj"

# Copy all source files
COPY ["Services/BuildingBlocks/", "Services/BuildingBlocks/"]
COPY ["Services/TimeCafe.ServiceDefaults/", "Services/TimeCafe.ServiceDefaults/"]
COPY ["Services/Audit/", "Services/Audit/"]

WORKDIR "/src/Services/Audit/Audit.TimeCafe.API"

FROM build AS publish
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "Audit.TimeCafe.API.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM build AS publish-debug
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "Audit.TimeCafe.API.csproj" -c Debug --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Audit.TimeCafe.API.dll"]

FROM base AS debug
WORKDIR /app
COPY --from=publish-debug /app/publish .

HEALTHCHECK NONE

ENTRYPOINT ["dotnet", "Audit.TimeCafe.API.dll"]
