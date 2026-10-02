<#
.SYNOPSIS
    Runs the DemoApp UI tests and opens the HTML dashboard when they finish.

.DESCRIPTION
    Convenience wrapper around `dotnet test` for local runs on Windows.
    Settings are passed as UITEST_ environment variables for this run only and
    restored afterwards. When the browser is visible (-Headed or -SlowMo), tests
    run one at a time so they are easy to follow.

.EXAMPLE
    .\run-tests.ps1                                   # all tests, headless, open report
.EXAMPLE
    .\run-tests.ps1 -Headed -SlowMo 500 -Filter "TestCategory=Smoke"
.EXAMPLE
    .\run-tests.ps1 -Browser Edge -Filter "Name~Logout"
.NOTES
    If scripts are blocked, run:
    powershell -ExecutionPolicy Bypass -File .\run-tests.ps1 -Headed -SlowMo 500
#>
[CmdletBinding()]
param(
    # dotnet test filter, e.g. "TestCategory=Smoke" or "Name~Logout".
    [string]$Filter = "",

    # Show the browser window.
    [switch]$Headed,

    # Pause (ms) with the element highlighted before every click/keystroke. Implies -Headed.
    [ValidateRange(0, 10000)]
    [int]$SlowMo = 0,

    [ValidateSet("Chrome", "Firefox", "Edge")]
    [string]$Browser = "Chrome",

    # Parallel test workers. 0 = automatic (1 when the browser is visible, otherwise the project default).
    [ValidateRange(0, 16)]
    [int]$Workers = 0,

    # Do not open the HTML report afterwards.
    [switch]$NoReport
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

if ($SlowMo -gt 0) { $Headed = $true }
if ($Workers -eq 0 -and $Headed) { $Workers = 1 }

$settings = @{
    "UITEST_Test__Browser"  = $Browser
    "UITEST_Test__Headless" = (-not $Headed).ToString().ToLowerInvariant()
    "UITEST_Test__SlowMoMs" = "$SlowMo"
}

$previous = @{}
foreach ($name in $settings.Keys) {
    $previous[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
    [Environment]::SetEnvironmentVariable($name, $settings[$name], "Process")
}

$arguments = @("test", (Join-Path $root "DemoApp.UiTests.sln"))
if ($Filter) { $arguments += @("--filter", $Filter) }
if ($Workers -gt 0) { $arguments += @("--", "NUnit.NumberOfTestWorkers=$Workers") }

Write-Host "Running: dotnet $($arguments -join ' ')" -ForegroundColor Cyan
Write-Host ("Browser: {0} | Headed: {1} | SlowMo: {2} ms | Workers: {3}" -f $Browser, $Headed, $SlowMo, $(if ($Workers -gt 0) { $Workers } else { "default" })) -ForegroundColor Cyan

try {
    & dotnet @arguments
    $exitCode = $LASTEXITCODE
}
finally {
    foreach ($name in $previous.Keys) {
        [Environment]::SetEnvironmentVariable($name, $previous[$name], "Process")
    }
}

$report = Join-Path $root "TestResults\report\index.html"
if (Test-Path $report) {
    Write-Host ""
    Write-Host "HTML report: $report" -ForegroundColor Green
    if (-not $NoReport) { Start-Process $report }
}
else {
    Write-Host "No HTML report was produced (did the build fail?)." -ForegroundColor Yellow
}

exit $exitCode
