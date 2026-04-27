param(
    [string]$MockServerUrl = "http://localhost:1080"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$expectationsPath = Join-Path $root "mockserver\expectations"
$expectations = @(
    "payment-success.json",
    "payment-error.json",
    "payment-timeout.json"
)

foreach ($expectation in $expectations) {
    $filePath = Join-Path $expectationsPath $expectation
    $body = Get-Content -Raw -Path $filePath

    Invoke-RestMethod -Method Put `
        -Uri "$MockServerUrl/mockserver/expectation" `
        -ContentType "application/json" `
        -Body $body | Out-Null

    Write-Host "Seeded $expectation"
}