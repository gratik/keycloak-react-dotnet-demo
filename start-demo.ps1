[CmdletBinding()]
param(
    [switch]$NoOpen,
    [switch]$SkipNpmInstall
)

$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RunDir = Join-Path $RootDir ".run"
$FrontendDir = Join-Path $RootDir "frontend"
$ComposeFile = Join-Path $RootDir "infra/docker-compose.yml"
$FrontendPidFile = Join-Path $RunDir "frontend.pid"
$FrontendLogFile = Join-Path $RunDir "frontend.log"

New-Item -ItemType Directory -Path $RunDir -Force | Out-Null

function Import-EnvFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        return
    }

    Get-Content -Path $Path | ForEach-Object {
        $line = $_.Trim()
        if (-not $line -or $line.StartsWith("#")) {
            return
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -lt 1) {
            return
        }

        $name = $line.Substring(0, $separatorIndex).Trim()
        $value = $line.Substring($separatorIndex + 1).Trim()
        [Environment]::SetEnvironmentVariable($name, $value)
    }
}

function Require-Command {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Missing required command: $Name"
    }
}

function Wait-ForHttp {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,
        [Parameter(Mandatory = $true)]
        [int]$ExpectedStatus,
        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing
            if ([int]$response.StatusCode -eq $ExpectedStatus) {
                Write-Host "$Label is ready at $Url"
                return
            }
        }
        catch {
            $statusCode = $null
            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $statusCode = [int]$_.Exception.Response.StatusCode
            }

            if ($statusCode -eq $ExpectedStatus) {
                Write-Host "$Label is ready at $Url"
                return
            }
        }

        Start-Sleep -Seconds 2
    }

    throw "Timed out waiting for $Label at $Url"
}

function Start-Frontend {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FrontendUrl
    )

    try {
        $existingResponse = Invoke-WebRequest -Uri $FrontendUrl -Method Get -UseBasicParsing -TimeoutSec 2
        if ([int]$existingResponse.StatusCode -eq 200) {
            Write-Host "Frontend dev server already responds at $FrontendUrl"
            return
        }
    }
    catch {
    }

    if (Test-Path $FrontendPidFile) {
        $existingPid = Get-Content $FrontendPidFile | Select-Object -First 1
        if ($existingPid) {
            $existingProcess = Get-Process -Id ([int]$existingPid) -ErrorAction SilentlyContinue
            if ($existingProcess) {
                Write-Host "Frontend dev server is already running with PID $existingPid"
                return
            }
        }

        Remove-Item $FrontendPidFile -ErrorAction SilentlyContinue
    }

    if ((-not (Test-Path (Join-Path $FrontendDir "node_modules"))) -and (-not $SkipNpmInstall)) {
        Write-Host "Installing frontend dependencies..."
        Push-Location $FrontendDir
        try {
            & npm install
        }
        finally {
            Pop-Location
        }
    }

    Write-Host "Starting frontend dev server..."
    $npmCommand = Get-Command npm.cmd -ErrorAction SilentlyContinue
    if (-not $npmCommand) {
        $npmCommand = Get-Command npm -ErrorAction SilentlyContinue
    }

    if (-not $npmCommand) {
        throw "Unable to resolve npm executable."
    }

    $process = Start-Process -FilePath $npmCommand.Source `
        -ArgumentList "run", "dev", "--", "--host", "127.0.0.1" `
        -WorkingDirectory $FrontendDir `
        -RedirectStandardOutput $FrontendLogFile `
        -RedirectStandardError $FrontendLogFile `
        -PassThru

    Set-Content -Path $FrontendPidFile -Value $process.Id
    Wait-ForHttp -Url $FrontendUrl -ExpectedStatus 200 -Label "Frontend"
}

function Open-BrowserIfNeeded {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    if ($NoOpen) {
        Write-Host "Skipping browser open because -NoOpen was specified"
        return
    }

    Start-Process $Url | Out-Null
}

Import-EnvFile -Path (Join-Path $RootDir ".env.example")
Import-EnvFile -Path (Join-Path $RootDir ".env")
Import-EnvFile -Path (Join-Path $FrontendDir ".env.example")
Import-EnvFile -Path (Join-Path $FrontendDir ".env")

if (-not $env:KEYCLOAK_PORT) { $env:KEYCLOAK_PORT = "8080" }
if (-not $env:API_HTTP_PORT) { $env:API_HTTP_PORT = "8081" }
if (-not $env:REPORTING_API_HTTP_PORT) { $env:REPORTING_API_HTTP_PORT = "8082" }
if (-not $env:VITE_KEYCLOAK_URL) { $env:VITE_KEYCLOAK_URL = "http://localhost:$($env:KEYCLOAK_PORT)" }
if (-not $env:VITE_KEYCLOAK_REALM) { $env:VITE_KEYCLOAK_REALM = "demo-realm" }
if (-not $env:VITE_KEYCLOAK_CLIENT_ID) { $env:VITE_KEYCLOAK_CLIENT_ID = "react-spa" }
if (-not $env:VITE_KEYCLOAK_REDIRECT_URI) { $env:VITE_KEYCLOAK_REDIRECT_URI = "http://localhost:5173/auth/callback" }
if (-not $env:VITE_API_BASE_URL) { $env:VITE_API_BASE_URL = "http://localhost:$($env:API_HTTP_PORT)" }
if (-not $env:VITE_REPORTING_API_BASE_URL) { $env:VITE_REPORTING_API_BASE_URL = "http://localhost:$($env:REPORTING_API_HTTP_PORT)" }

Require-Command -Name "docker"
Require-Command -Name "node"
Require-Command -Name "npm"

try {
    & docker info | Out-Null
}
catch {
    throw "Docker is not available. Start Docker Desktop and try again."
}

if (-not (Test-Path $ComposeFile)) {
    throw "Missing compose file: $ComposeFile"
}

if (-not (Test-Path (Join-Path $FrontendDir "package.json"))) {
    throw "Missing frontend package.json"
}

$requiredValues = @{
    KEYCLOAK_PORT = $env:KEYCLOAK_PORT
    API_HTTP_PORT = $env:API_HTTP_PORT
    REPORTING_API_HTTP_PORT = $env:REPORTING_API_HTTP_PORT
    VITE_KEYCLOAK_URL = $env:VITE_KEYCLOAK_URL
    VITE_KEYCLOAK_REALM = $env:VITE_KEYCLOAK_REALM
    VITE_KEYCLOAK_CLIENT_ID = $env:VITE_KEYCLOAK_CLIENT_ID
    VITE_KEYCLOAK_REDIRECT_URI = $env:VITE_KEYCLOAK_REDIRECT_URI
    VITE_API_BASE_URL = $env:VITE_API_BASE_URL
    VITE_REPORTING_API_BASE_URL = $env:VITE_REPORTING_API_BASE_URL
}

foreach ($entry in $requiredValues.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace($entry.Value)) {
        throw "Missing required configuration: $($entry.Key)"
    }
}

Write-Host "Validated configuration."
Write-Host "Keycloak URL: $($env:VITE_KEYCLOAK_URL)"
Write-Host "API 1 URL: $($env:VITE_API_BASE_URL)"
Write-Host "API 2 URL: $($env:VITE_REPORTING_API_BASE_URL)"
Write-Host "Frontend URL: http://localhost:5173"

Write-Host "Starting Docker services..."
& docker compose -f $ComposeFile up -d --build

Wait-ForHttp -Url "$($env:VITE_KEYCLOAK_URL)/realms/$($env:VITE_KEYCLOAK_REALM)/.well-known/openid-configuration" -ExpectedStatus 200 -Label "Keycloak"
Wait-ForHttp -Url "$($env:VITE_API_BASE_URL)/api/demo/protected" -ExpectedStatus 401 -Label "API"
Wait-ForHttp -Url "$($env:VITE_REPORTING_API_BASE_URL)/api/reports/summary" -ExpectedStatus 401 -Label "Reporting API"

Start-Frontend -FrontendUrl "http://localhost:5173"
Open-BrowserIfNeeded -Url "http://localhost:5173"

Write-Host ""
Write-Host "Demo is ready."
Write-Host "Frontend log: $FrontendLogFile"
Write-Host "Use alice / Passw0rd! or bob / Passw0rd! on the Keycloak login page."
