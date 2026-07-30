$ErrorActionPreference = "Stop"

$rootPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiPath = Join-Path $rootPath "src\Elevating.Api"
$webPath = Join-Path $rootPath "src\Elevating.Web"

if (-not (Test-Path $apiPath)) {
    Write-Error "API project folder was not found: $apiPath"
    exit 1
}

if (-not (Test-Path $webPath)) {
    Write-Error "Angular project folder was not found: $webPath"
    exit 1
}

$apiProject = Get-ChildItem `
    -Path $apiPath `
    -Filter "*.csproj" `
    -File |
    Select-Object -First 1

if (-not $apiProject) {
    Write-Error "No .csproj file was found in: $apiPath"
    exit 1
}

if (-not (Test-Path (Join-Path $webPath "package.json"))) {
    Write-Error "No package.json file was found in: $webPath"
    exit 1
}

Write-Host ""
Write-Host "Starting Elevating development environment..." -ForegroundColor Cyan
Write-Host "API:     https://localhost:7269" -ForegroundColor DarkCyan
Write-Host "Angular: http://localhost:4200" -ForegroundColor DarkCyan
Write-Host ""

Start-Process powershell.exe -ArgumentList @(
    "-NoExit"
    "-Command"
    "& {
        Set-Location '$apiPath'
        `$Host.UI.RawUI.WindowTitle = 'Elevating API'
        dotnet watch run --launch-profile https
    }"
)

Start-Sleep -Seconds 3

Start-Process powershell.exe -ArgumentList @(
    "-NoExit"
    "-Command"
    "& {
        Set-Location '$webPath'
        `$Host.UI.RawUI.WindowTitle = 'Elevating Angular'
        npm start
    }"
)

Write-Host "API and Angular terminals opened successfully." -ForegroundColor Green