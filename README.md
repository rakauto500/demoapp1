# DemoApp – Selenium UI Test Automation (C# / .NET 8 / NUnit)

[![CI](https://github.com/rakauto500/demoapp1/actions/workflows/ci.yml/badge.svg)](https://github.com/rakauto500/demoapp1/actions/workflows/ci.yml)
[![CD](https://github.com/rakauto500/demoapp1/actions/workflows/cd.yml/badge.svg)](https://github.com/rakauto500/demoapp1/actions/workflows/cd.yml)
[![Nightly regression](https://github.com/rakauto500/demoapp1/actions/workflows/nightly.yml/badge.svg)](https://github.com/rakauto500/demoapp1/actions/workflows/nightly.yml)

- Live app: https://rakauto500.github.io/demoapp1/
- Latest test dashboard: https://rakauto500.github.io/demoapp1/report/

End-to-end UI tests for a small demo web app, built with **Selenium WebDriver 4**,
**NUnit 5** and the **Page Object Model** on **.NET 8**.

📄 Full architecture, decisions, alternatives and implementation notes:
[`docs/Selenium-Test-Automation-Design.odt`](docs/Selenium-Test-Automation-Design.odt)
(OpenDocument – edit with LibreOffice / OpenOffice Writer).

## Quick start

Prerequisites: .NET 8 SDK and Chrome (Firefox/Edge optional).

```bash
dotnet test                                    # build + run all 26 tests (headless Chrome)
dotnet test --filter "TestCategory=Smoke"      # smoke tests only
UITEST_Test__Headless=false dotnet test        # watch the browser
UITEST_Test__Headless=false UITEST_Test__SlowMoMs=500 dotnet test -- NUnit.NumberOfTestWorkers=1  # slow motion
UITEST_Test__Browser=Firefox dotnet test       # another browser
UITEST_Test__BaseUrl=https://staging.example.com/ dotnet test   # deployed env
```

### Windows: one-command runs (watch the browser + open the dashboard)

```powershell
.\run-tests.ps1 -Headed -SlowMo 500 -Filter "TestCategory=Smoke"   # watch 2 tests in slow motion
.\run-tests.ps1 -Headed -SlowMo 500                                 # watch everything
.\run-tests.ps1                                                     # fast headless run
```

`-SlowMo` pauses (ms) and highlights each element before every click and keystroke; visible runs go
one test at a time. If PowerShell says *running scripts is disabled*, either run
`powershell -ExecutionPolicy Bypass -File .\run-tests.ps1 -Headed -SlowMo 500`, or allow local
scripts once for your user: `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`.

### HTML dashboard

Every run writes **`TestResults/report/index.html`** at the repository root: pass rate, results by
category, slowest tests, run history (last 30 runs, from `history.json`), a searchable table, and
failure details with embedded screenshots. It is a single file, so open it straight from disk
(`start TestResults\report\index.html`) or set `UITEST_Test__OpenReport=true` to open it
automatically. In CI it is part of the `ui-test-results` artifact.

The suite starts its own embedded web server for `app/`, so no other setup is needed.
Selenium Manager downloads a matching browser driver on first run; on locked-down networks set
`UITEST_Test__DriverPath` (and optionally `UITEST_Test__BrowserBinaryPath`).

Failure screenshots and page HTML are written to
`tests/DemoApp.UiTests/bin/<config>/net8.0/TestResults/artifacts/`.

## Test data (CSV)

All tests are data driven: inputs and expected results live in CSV files in
[`tests/DemoApp.UiTests/TestData/Csv/`](tests/DemoApp.UiTests/TestData/Csv/README.md) – one file per
scenario, one row per test case. Add a row → you get a new test; no code changes. See the README in
that folder for the columns of each file.

## CI/CD (GitHub Actions)

| Workflow | When | What |
|----------|------|------|
| `ci.yml` – **CI** | Every pull request and push to a non-main branch | Build + all tests against the embedded server; TRX, dashboard and failure screenshots as artifacts |
| `cd.yml` – **CD** | Every push/merge to `main` | 1. Test gate (embedded server) → 2. deploy `app/` to GitHub Pages and wait until live → 3. all tests against the live URL → 4. publish the dashboard (with run history) to `/report/` |
| `nightly.yml` – **Nightly regression** | 03:17 UTC daily (+ manual) | All tests against the live site; dashboard republished |
| `live-tests.yml` | Called by CD and Nightly | Shared "test live site + publish dashboard" jobs |

One-time setup: **Settings → Pages → Build and deployment → Source: GitHub Actions**. GitHub Pages on a
private repository needs GitHub Pro/Team (or make the repository public).

## Layout

```
app/                         System under test (static HTML/JS)
tests/DemoApp.UiTests/
  Config/      Typed settings (appsettings.json + UITEST_ env vars)
  Drivers/     WebDriverFactory (Chrome / Edge / Firefox)
  Infrastructure/  Embedded static file server
  Pages/       Page objects (BasePage, LoginPage, DashboardPage)
  Reporting/   HTML dashboard writer + template
  TestData/    CSV loader, [CsvData] attribute, row records
    Csv/       *.csv test data (edit these to add/change test cases)
  Tests/       BaseTest + LoginTests + TodoTests
docs/        Design document (.odt)
run-tests.ps1  Windows launcher (headed / slow motion / open report)
.github/workflows/  ci.yml, cd.yml, nightly.yml, live-tests.yml
```
