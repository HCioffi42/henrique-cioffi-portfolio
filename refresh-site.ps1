# refresh-site.ps1
# HC: Automated refresh for the local Docker environment

Clear-Host
$CurrentDir = Get-Location

if (!(Test-Path "docker-compose.yml")) {
    Write-Host "Error: docker-compose.yml not found in $CurrentDir" -ForegroundColor Red
    Write-Host "Please run this script from the project root."
    Pause
    exit
}

Write-Host "--- Restarting Environment: MeuSitePessoal ---" -ForegroundColor Cyan

Write-Host "1. Stopping and cleaning containers..." -ForegroundColor Yellow
docker-compose down --remove-orphans

Write-Host "2. Building images from scratch (No Cache)..." -ForegroundColor Yellow
docker-compose build --no-cache

Write-Host "3. Lifting orchestration..." -ForegroundColor Yellow
docker-compose up -d

Write-Host "`n--- Success! ---" -ForegroundColor Green
Write-Host "Local: http://localhost"
Write-Host "Network: http://192.168.3.106" -ForegroundColor White
Write-Host "--------------------------------------------"
Pause