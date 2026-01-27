FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8001
ENV ASPNETCORE_URLS=http://+:8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy BuildingBlocks
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

# Copy Auth service projects
COPY ["Services/Auth/Auth.TimeCafe.API/Auth.TimeCafe.API.csproj", "Services/Auth/Auth.TimeCafe.API/"]
COPY ["Services/Auth/Auth.TimeCafe.Application/Auth.TimeCafe.Application.csproj", "Services/Auth/Auth.TimeCafe.Application/"]
COPY ["Services/Auth/Auth.TimeCafe.Domain/Auth.TimeCafe.Domain.csproj", "Services/Auth/Auth.TimeCafe.Domain/"]
COPY ["Services/Auth/Auth.TimeCafe.Infrastructure/Auth.TimeCafe.Infrastructure.csproj", "Services/Auth/Auth.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Services/Auth/Auth.TimeCafe.API/Auth.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/Auth/", "Services/Auth/"]

# Build
WORKDIR "/src/Services/Auth/Auth.TimeCafe.API"
RUN dotnet build "Auth.TimeCafe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.TimeCafe.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8001/health || exit 1

ENTRYPOINT ["dotnet", "Auth.TimeCafe.API.dll"]
