using CsvIgnore = CsvHelper.Configuration.Attributes.IgnoreAttribute;
using DemoApp.UiTests.Pages;

namespace DemoApp.UiTests.TestData;

// One record per scenario CSV file (TestData/Csv). Column names match property names
// (case-insensitive). List cells use "|" between values.

/// <summary>users.csv — named accounts used by tests that just need to be logged in.</summary>
public sealed record UserRecord
{
    public string Role { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

/// <summary>login_valid.csv</summary>
public sealed record ValidLoginCase : CsvCase
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ExpectedWelcomeName { get; init; } = string.Empty;

    public string ExpectedTitle { get; init; } = string.Empty;
}

/// <summary>login_invalid.csv</summary>
public sealed record InvalidLoginCase : CsvCase
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string ExpectedError { get; init; } = string.Empty;
}

/// <summary>todo_add.csv</summary>
public sealed record AddTodoCase : CsvCase
{
    public string Todos { get; init; } = string.Empty;

    public int ExpectedItemsLeft { get; init; }

    [CsvIgnore]
    public IReadOnlyList<string> TodoList => List(Todos);
}

/// <summary>todo_blank.csv</summary>
public sealed record BlankTodoCase : CsvCase
{
    public string Todo { get; init; } = string.Empty;
}

/// <summary>todo_complete.csv</summary>
public sealed record CompleteTodoCase : CsvCase
{
    public string Todos { get; init; } = string.Empty;

    public string Complete { get; init; } = string.Empty;

    public int ExpectedItemsLeft { get; init; }

    [CsvIgnore]
    public IReadOnlyList<string> TodoList => List(Todos);

    [CsvIgnore]
    public IReadOnlyList<string> CompleteList => List(Complete);
}

/// <summary>todo_delete.csv</summary>
public sealed record DeleteTodoCase : CsvCase
{
    public string Todos { get; init; } = string.Empty;

    public string Delete { get; init; } = string.Empty;

    public string ExpectedRemaining { get; init; } = string.Empty;

    [CsvIgnore]
    public IReadOnlyList<string> TodoList => List(Todos);

    [CsvIgnore]
    public IReadOnlyList<string> ExpectedRemainingList => List(ExpectedRemaining);
}

/// <summary>todo_filter.csv</summary>
public sealed record FilterTodoCase : CsvCase
{
    public string Todos { get; init; } = string.Empty;

    public string Complete { get; init; } = string.Empty;

    public TodoFilter Filter { get; init; }

    public string ExpectedVisible { get; init; } = string.Empty;

    [CsvIgnore]
    public IReadOnlyList<string> TodoList => List(Todos);

    [CsvIgnore]
    public IReadOnlyList<string> CompleteList => List(Complete);

    [CsvIgnore]
    public IReadOnlyList<string> ExpectedVisibleList => List(ExpectedVisible);
}

/// <summary>todo_refresh.csv</summary>
public sealed record RefreshTodoCase : CsvCase
{
    public string Todos { get; init; } = string.Empty;

    [CsvIgnore]
    public IReadOnlyList<string> TodoList => List(Todos);
}
