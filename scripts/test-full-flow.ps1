param(
    [string]$ApiUrl = "https://localhost:5001",
    [int]$TimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

Write-Host "=== PIX Integration Hub - Full Test Flow ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Start MockServer
Write-Host "[1/4] Starting MockServer..." -ForegroundColor Yellow
$mockServerRunning = docker compose ps mock-server --services 2>$null | Select-String "mock-server"

if ($mockServerRunning) {
    Write-Host "MockServer is already running." -ForegroundColor Green
} else {
    Write-Host "Bringing up MockServer..." -ForegroundColor White
    docker compose up -d mock-server | Out-Null
    Start-Sleep -Seconds 3
    Write-Host "MockServer started." -ForegroundColor Green
}

Write-Host ""

# Step 2: Seed expectations
Write-Host "[2/4] Seeding MockServer expectations..." -ForegroundColor Yellow
try {
    &"$PSScriptRoot\seed-mockserver.ps1" | ForEach-Object { Write-Host $_ -ForegroundColor White }
    Write-Host "Expectations seeded successfully." -ForegroundColor Green
} catch {
    Write-Host "Error seeding expectations: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Start API (background job)
Write-Host "[3/4] Starting API in background..." -ForegroundColor Yellow

$job = Start-Job -ScriptBlock {
    cd "$using:PSScriptRoot\..\ApiService"
    dotnet run --launch-profile https --no-build 2>&1
} -Name "ApiService"

Write-Host "Waiting for API to start..." -ForegroundColor White
$apiReady = $false
$attempts = 0
$maxAttempts = 60

while (-not $apiReady -and $attempts -lt $maxAttempts) {
    try {
        $response = Invoke-RestMethod -Method Get -Uri "$ApiUrl/weatherforecast" -SkipCertificateCheck -ErrorAction SilentlyContinue
        if ($response) {
            $apiReady = $true
        }
    } catch {
        # API not ready yet
    }
    
    if (-not $apiReady) {
        Start-Sleep -Seconds 1
        $attempts++
    }
}

if ($apiReady) {
    Write-Host "API is ready." -ForegroundColor Green
} else {
    Write-Host "API failed to start after $maxAttempts attempts." -ForegroundColor Red
    Stop-Job -Job $job
    exit 1
}

Write-Host ""

# Step 4: Run tests
Write-Host "[4/4] Testing payment scenarios..." -ForegroundColor Yellow
Write-Host ""

$scenarios = @(
    @{
        Name = "Success Scenario"
        Body = @{
            amount = 100.50
            merchantId = "merchant-01"
            scenario = 0
        }
        Expected = "approved"
        Color = "Green"
    },
    @{
        Name = "Error Scenario"
        Body = @{
            amount = 100.50
            merchantId = "merchant-01"
            scenario = 1
        }
        Expected = "error"
        Color = "Red"
    },
    @{
        Name = "Timeout Scenario"
        Body = @{
            amount = 100.50
            merchantId = "merchant-01"
            scenario = 2
        }
        Expected = "approved"
        Color = "Yellow"
    }
)

$testsPassed = 0
$testsFailed = 0

foreach ($scenario in $scenarios) {
    Write-Host "Testing: $($scenario.Name)" -ForegroundColor $scenario.Color
    
    try {
        $body = $scenario.Body | ConvertTo-Json
        $response = Invoke-RestMethod -Method Post `
            -Uri "$ApiUrl/payments/mock" `
            -ContentType "application/json" `
            -Body $body `
            -SkipCertificateCheck `
            -TimeoutSec $TimeoutSeconds
        
        Write-Host "  Response: $(ConvertTo-Json $response)" -ForegroundColor White
        
        if ($response.status -match $scenario.Expected) {
            Write-Host "  PASSED" -ForegroundColor Green
            $testsPassed++
        } else {
            Write-Host "  FAILED - Expected status containing '$($scenario.Expected)' but got '$($response.status)'" -ForegroundColor Red
            $testsFailed++
        }
    } catch {
        Write-Host "  FAILED - $_" -ForegroundColor Red
        $testsFailed++
    }
    
    Write-Host ""
}

# Cleanup
Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "Passed: $testsPassed" -ForegroundColor Green
Write-Host "Failed: $testsFailed" -ForegroundColor Green
Write-Host ""

Write-Host "Stopping API..." -ForegroundColor Yellow
Stop-Job -Job $job
Remove-Job -Job $job

Write-Host "Done!" -ForegroundColor Cyan
