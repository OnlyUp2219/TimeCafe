FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8002
ENV ASPNETCORE_URLS=http://+:8002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy BuildingBlocks
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

# Copy UserProfile service projects
COPY ["Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.API/UserProfile.TimeCafe.API.csproj", "Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.API/"]
COPY ["Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.Application/UserProfile.TimeCafe.Application.csproj", "Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.Application/"]
COPY ["Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.Domain/UserProfile.TimeCafe.Domain.csproj", "Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.Domain/"]
COPY ["Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.Infrastructure/UserProfile.TimeCafe.Infrastructure.csproj", "Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.API/UserProfile.TimeCafe.API.csproj"

# Copy all source files
COPY ["BuildingBlocks/", "BuildingBlocks/"]
COPY ["Services/UserProfile/", "Services/UserProfile/"]

# Build
WORKDIR "/src/Services/UserProfile/UserProfile.TimeCafe/UserProfile.TimeCafe.API"
RUN dotnet build "UserProfile.TimeCafe.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserProfile.TimeCafe.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check  
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8002/health || exit 1

ENTRYPOINT ["dotnet", "UserProfile.TimeCafe.API.dll"]
