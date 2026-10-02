# Test data (CSV)

Every data-driven test reads its rows from a file in this folder: **one row = one test case**.
Edit the files in Excel, LibreOffice Calc or any text editor, then run the tests – no code changes.

## Rules

- First row is the header. Column names must match exactly (case does not matter).
- `CaseName` – unique, short, no spaces (e.g. `WrongPassword`). It becomes the test name:
  `InvalidLogin_ShowsExpectedError(WrongPassword)`.
- `Categories` – optional NUnit categories, separated by `;` (e.g. `Smoke;Regression`).
  Run them with `dotnet test --filter "TestCategory=Smoke"`.
- Lists in one cell use `|` between values: `Plan|Build|Test`. An empty cell is an empty list.
- Leave a cell empty for an empty value. Wrap values with leading/trailing spaces or commas in
  double quotes: `"   "`, `"Smith, John"`.
- Lines starting with `#` are comments.
- From Excel: **File → Save As → CSV UTF-8 (Comma delimited)**.

## Files

| File | Test | Columns |
|------|------|---------|
| `users.csv` | Accounts for tests that only need to be logged in | `Role, Username, Password` |
| `login_valid.csv` | `ValidLogin_ShowsDashboard` | `CaseName, Username, Password, ExpectedWelcomeName, ExpectedTitle, Categories` |
| `login_invalid.csv` | `InvalidLogin_ShowsExpectedError` | `CaseName, Username, Password, ExpectedError, Categories` |
| `todo_add.csv` | `AddTodos_AppearInOrder_AndCounterMatches` | `CaseName, Todos, ExpectedItemsLeft, Categories` |
| `todo_blank.csv` | `BlankTodo_IsNotAdded` | `CaseName, Todo, Categories` |
| `todo_complete.csv` | `CompleteTodos_StrikeThrough_AndCounterDecrements` | `CaseName, Todos, Complete, ExpectedItemsLeft, Categories` |
| `todo_delete.csv` | `DeleteTodo_RemovesOnlyThatItem` | `CaseName, Todos, Delete, ExpectedRemaining, Categories` |
| `todo_filter.csv` | `Filter_ShowsOnlyMatchingTodos` (`Filter` = All, Active or Completed) | `CaseName, Todos, Complete, Filter, ExpectedVisible, Categories` |
| `todo_refresh.csv` | `Todos_ArePreserved_AfterPageRefresh` | `CaseName, Todos, Categories` |

A different data set (e.g. staging accounts) can be used without touching these files:
`UITEST_Test__TestDataDirectory=C:\data\staging dotnet test`.
