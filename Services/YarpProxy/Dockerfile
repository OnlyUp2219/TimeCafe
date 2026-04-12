# syntax=docker/dockerfile:1.7
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8010
ENV ASPNETCORE_URLS=http://+:8010

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8010/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Central package versions (required for restore)
COPY ["Directory.Packages.props", "./"]

# Copy BuildingBlocks
COPY ["Services/BuildingBlocks/BuildingBlocks.csproj", "Services/BuildingBlocks/"]

# Copy YarpProxy project
COPY ["Services/YarpProxy/YarpProxy.csproj", "Services/YarpProxy/"]

# Restore dependencies
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet restore "Services/YarpProxy/YarpProxy.csproj"

# Copy all source files
COPY ["Services/BuildingBlocks/", "Services/BuildingBlocks/"]
COPY ["Services/YarpProxy/", "Services/YarpProxy/"]

WORKDIR "/src/Services/YarpProxy"

FROM build AS publish
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "YarpProxy.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM build AS publish-debug
RUN --mount=type=cache,id=timecafe-nuget-packages,target=/root/.nuget/packages,sharing=locked \
    --mount=type=cache,id=timecafe-nuget-cache,target=/root/.local/share/NuGet,sharing=locked \
    dotnet publish "YarpProxy.csproj" -c Debug --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["appsettings.shared.json", "/app/"]

ENTRYPOINT ["dotnet", "YarpProxy.dll"]

FROM base AS debug
WORKDIR /app
COPY --from=publish-debug /app/publish .
COPY ["appsettings.shared.json", "/app/"]

HEALTHCHECK NONE

ENTRYPOINT ["dotnet", "YarpProxy.dll"]
