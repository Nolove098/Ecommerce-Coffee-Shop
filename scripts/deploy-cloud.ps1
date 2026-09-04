param(
    [string]$PublicUrl
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot

try {
    dotnet restore .\SaleStore.csproj
    dotnet build .\SaleStore.csproj -c Release --no-restore
    dotnet publish .\SaleStore.csproj -c Release --no-restore -o .\artifacts\publish

    railway up --service coffeeshop-demo --detach

    if ($PublicUrl) {
        $healthUrl = $PublicUrl.TrimEnd('/') + '/health'
        $healthy = $false
        for ($attempt = 1; $attempt -le 20; $attempt++) {
            try {
                $response = Invoke-WebRequest -Uri $healthUrl -Method Get -TimeoutSec 15
                if ($response.StatusCode -eq 200) {
                    $healthy = $true
                    break
                }
            }
            catch {
                # Railway can continue serving the prior deployment while the new
                # image builds. Retry without logging response bodies or settings.
            }
            Start-Sleep -Seconds 5
        }
        if (-not $healthy) {
            throw "Cloud health validation did not reach HTTP 200."
        }
    }
}
finally {
    Pop-Location
}
