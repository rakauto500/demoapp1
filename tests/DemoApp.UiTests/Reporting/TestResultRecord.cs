namespace DemoApp.UiTests.Reporting;

/// <summary>One executed test, as shown on the HTML dashboard.</summary>
public sealed record TestResultRecord
{
    public required string Name { get; init; }

    public required string FullName { get; init; }

    public required string Fixture { get; init; }

    public required IReadOnlyList<string> Categories { get; init; }

    /// <summary>Passed, Failed, Skipped or Inconclusive.</summary>
    public required string Outcome { get; init; }

    public required double DurationSeconds { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public string? Message { get; init; }

    public string? StackTrace { get; init; }

    /// <summary>PNG screenshot at failure, base64-encoded so the report is a single file.</summary>
    public string? ScreenshotBase64 { get; init; }
}
