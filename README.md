# DemoApp – Selenium UI Test Automation (C# / .NET 8 / NUnit)

End-to-end UI tests for a small demo web app, built with **Selenium WebDriver 4**,
**NUnit 5** and the **Page Object Model** on **.NET 8**.

📄 Full architecture, decisions, alternatives and implementation notes:
[`docs/Selenium-Test-Automation-Design.odt`](docs/Selenium-Test-Automation-Design.odt)
(OpenDocument – edit with LibreOffice / OpenOffice Writer).

## Quick start

Prerequisites: .NET 8 SDK and Chrome (Firefox/Edge optional).

```bash
dotnet test                                    # build + run all 16 tests (headless Chrome)
dotnet test --filter "TestCategory=Smoke"      # smoke tests only
UITEST_Test__Headless=false dotnet test        # watch the browser
UITEST_Test__Browser=Firefox dotnet test       # another browser
UITEST_Test__BaseUrl=https://staging.example.com/ dotnet test   # deployed env
```

The suite starts its own embedded web server for `app/`, so no other setup is needed.
Selenium Manager downloads a matching browser driver on first run; on locked-down networks set
`UITEST_Test__DriverPath` (and optionally `UITEST_Test__BrowserBinaryPath`).

Failure screenshots and page HTML are written to
`tests/DemoApp.UiTests/bin/<config>/net8.0/TestResults/artifacts/`.

## Layout

```
app/                         System under test (static HTML/JS)
tests/DemoApp.UiTests/
  Config/      Typed settings (appsettings.json + UITEST_ env vars)
  Drivers/     WebDriverFactory (Chrome / Edge / Firefox)
  Infrastructure/  Embedded static file server
  Pages/       Page objects (BasePage, LoginPage, DashboardPage)
  TestData/    Test accounts
  Tests/       BaseTest + LoginTests + TodoTests
docs/        Design document (.odt)
.github/workflows/ui-tests.yml   CI on every push / PR
```
