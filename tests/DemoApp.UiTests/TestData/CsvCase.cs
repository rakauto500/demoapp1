using CsvHelper.Configuration.Attributes;

namespace DemoApp.UiTests.TestData;

/// <summary>
/// Base for one row of a scenario CSV file. Every scenario file has a unique
/// <c>CaseName</c> (shown as the test name) and an optional <c>Categories</c>
/// column (semicolon-separated NUnit categories, e.g. <c>Smoke;Regression</c>).
/// </summary>
public abstract record CsvCase
{
    public string CaseName { get; init; } = string.Empty;

    [Optional]
    public string Categories { get; init; } = string.Empty;

    /// <summary>Splits a pipe-separated cell ("A|B|C") into values; an empty cell is an empty list.</summary>
    protected static IReadOnlyList<string> List(string cell) =>
        cell.Split('|', StringSplitOptions.RemoveEmptyEntries);
}
