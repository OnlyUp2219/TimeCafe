# Переходим в корень (на один уровень выше папки scripts)
Set-Location "$PSScriptRoot\.."
Write-Host "Current Root: $(Get-Location)" -ForegroundColor Yellow

$services = @(
    @("Auth", "Services\Auth\Auth.TimeCafe.Infrastructure", "Services\Auth\Auth.TimeCafe.API"),
    @("Billing", "Services\Billing\Billing.TimeCafe.Infrastructure", "Services\Billing\Billing.TimeCafe.API"),
    @("Venue", "Services\Venue\Venue.TimeCafe.Infrastructure", "Services\Venue\Venue.TimeCafe.API"),
    @("UserProfile", "Services\UserProfile\UserProfile.TimeCafe.Infrastructure", "Services\UserProfile\UserProfile.TimeCafe.API")
)

foreach ($service in $services) {
    $name  = $service[0]
    $infra = $service[1]
    $api   = $service[2]

    Write-Host "`n--- Checking service: $name ---" -ForegroundColor Cyan

    if (-not (Test-Path $infra)) { 
        Write-Host "[ERROR] Path not found: $infra" -ForegroundColor Red
        continue 
    }

    Write-Host "Creating migration for $name..." -ForegroundColor Gray
    
    dotnet ef migrations add AddMassTransitOutbox --project $infra --startup-project $api
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Updating database for $name..." -ForegroundColor Gray
        dotnet ef database update --project $infra --startup-project $api
    }
    else {
        Write-Host "Failed to create migration for $name. Check if project builds or migration exists." -ForegroundColor Red
    }
}

Write-Host "`nFinished!" -ForegroundColor Green