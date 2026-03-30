# stop-site.ps1
# HC: Professional stop script with error trapping to prevent window auto-closing.

try {
    Write-Host "--- Starting Stop Sequence ---" -ForegroundColor Cyan
    
    # HC: Check if we are in the right directory
    if (!(Test-Path "docker-compose.yml")) {
        throw "Error: docker-compose.yml not found in $(Get-Location). Make sure the script is in the project root."
    }

    Write-Host "Executing docker-compose down..." -ForegroundColor Yellow
    docker-compose down --remove-orphans
    
    Write-Host "--- Environment Stopped Successfully ---" -ForegroundColor Green
}
catch {
    Write-Host "`n--- FATAL ERROR ---" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host "-------------------" -ForegroundColor Red
}

Write-Host "`nScript finished. Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")