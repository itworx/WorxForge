$scriptRoot = $PSScriptRoot
$apiProject = Join-Path $scriptRoot "..\WorxForge.API\WorxForge.API.csproj"
$swaggerUrl  = "http://localhost:5222/swagger/v1/swagger.json"
$nswagConfig = Join-Path $scriptRoot "nswag.json"

Write-Host "Starting API..." -ForegroundColor Cyan
$api = Start-Process dotnet -ArgumentList "run --project `"$apiProject`"" -PassThru -WindowStyle Hidden

try {
    Write-Host "Waiting for Swagger endpoint..." -ForegroundColor Cyan
    $ready = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 2
        try {
            $null = Invoke-WebRequest $swaggerUrl -UseBasicParsing -ErrorAction Stop
            $ready = $true
            break
        } catch { }
    }

    if (-not $ready) { throw "API did not start in time." }

    Write-Host "Generating client..." -ForegroundColor Cyan
    Push-Location $scriptRoot
    try {
        nswag run $nswagConfig
    } finally {
        Pop-Location
    }
    Write-Host "Done. Client written to src/api/worxforgeClient.ts" -ForegroundColor Green
} finally {
    Stop-Process -Id $api.Id -Force -ErrorAction SilentlyContinue
}
