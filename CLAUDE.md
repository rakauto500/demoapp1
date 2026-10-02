# Project rules

## Stack
C# / .NET 8, NUnit 5, Selenium WebDriver 4, Page Object Model. Build with warnings as errors.
Run `dotnet build` and `dotnet test` before every commit; all tests must pass.

## Conventions
- Tests contain scenarios and assertions only; locators and waits live in page objects (`Pages/`).
- Data-driven: all test inputs and expected values come from CSV files in
  `tests/DemoApp.UiTests/TestData/Csv/` (one file per scenario, one row per test case, unique
  `CaseName`, optional `Categories`). Bind with `[CsvData<TCase>("file.csv")]` and a record in
  `TestData/Cases.cs`; logged-in setup uses `Users.Standard` from `users.csv`. No data literals in tests.
  Keep `TestData/Csv/README.md` in sync when adding or changing a CSV file.
- Locate elements by `data-test` attributes via `BasePage.TestId()`.
- Explicit waits only: no `Thread.Sleep`, implicit wait stays 0. The single exception is the
  opt-in visual delay in `Drivers/SlowMotion.cs` (`SlowMoMs`, default 0); never use it for synchronisation.
- Test results are recorded by `BaseTest` for the HTML dashboard (`Reporting/`); keep new
  fixtures on `BaseTest` so they appear in the report.
- Page methods that navigate return the next page object; page objects never assert.
- Every test fixture derives from `BaseTest` and is `[Parallelizable(ParallelScope.All)]`.
- `TestEnvironment` ([SetUpFixture]) must stay in the root namespace `DemoApp.UiTests`.

## Living design document (mandatory)
`docs/Selenium-Test-Automation-Design.odt` is the master design document and must stay current.
In the same commit as any change to architecture, dependencies/versions, configuration,
CI, or the test catalogue:
1. Update the affected sections (decisions = section 6 "D-numbers", rejected alternatives =
   section 7, test catalogue = section 9, configuration = section 10).
2. Never delete a decision; mark it "Superseded by Dn".
3. Add a revision-history row and bump "Current version" / "Last updated".
4. Keep the file in ODF Text (.odt) format. When editing programmatically, modify
   `content.xml` inside the .odt (preserving existing styles) or use LibreOffice headless;
   then render to PDF and visually check the result before committing.
