using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DemoApp.UiTests.Config;

namespace DemoApp.UiTests.Reporting;

/// <summary>
/// Writes a single-file HTML dashboard (index.html) plus a rolling history.json
/// after every run. The page needs no server or internet: data, styles, script and
/// failure screenshots are all embedded.
/// </summary>
public static class HtmlReportWriter
{
    private const int HistoryLength = 30;
    private const string DataPlaceholder = "\"__REPORT_DATA__\"";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string Write(TestSettings settings, string baseUrl, IReadOnlyList<TestResultRecord> results, DateTimeOffset startedAt)
    {
        var directory = ResolveReportDirectory(settings.ReportDirectory);
        Directory.CreateDirectory(directory);

        var finishedAt = DateTimeOffset.Now;
        var run = new RunSummary
        {
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            DurationSeconds = Math.Round((finishedAt - startedAt).TotalSeconds, 2),
            Total = results.Count,
            Passed = results.Count(r => r.Outcome == "Passed"),
            Failed = results.Count(r => r.Outcome == "Failed"),
            Skipped = results.Count(r => r.Outcome is "Skipped" or "Inconclusive"),
            Browser = settings.Browser.ToString(),
            Headless = settings.Headless,
            BaseUrl = baseUrl,
            Machine = Environment.MachineName,
            Os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
        };

        var historyPath = Path.Combine(directory, "history.json");
        var history = LoadHistory(historyPath);
        history.Add(run);
        history = history.TakeLast(HistoryLength).ToList();
        File.WriteAllText(historyPath, JsonSerializer.Serialize(history, JsonOptions));

        var payload = new
        {
            run,
            results = results.OrderBy(r => r.Fixture).ThenBy(r => r.Name),
            history,
        };

        var html = LoadTemplate().Replace(DataPlaceholder, JsonSerializer.Serialize(payload, JsonOptions), StringComparison.Ordinal);
        var reportPath = Path.Combine(directory, "index.html");
        File.WriteAllText(reportPath, html);

        if (settings.OpenReport)
        {
            TryOpen(reportPath);
        }

        return reportPath;
    }

    /// <summary>Relative paths resolve from the repository root (folder containing the .sln).</summary>
    private static string ResolveReportDirectory(string configured)
    {
        if (Path.IsPathRooted(configured))
        {
            return configured;
        }

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.sln").Length == 0)
        {
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(dir?.FullName ?? AppContext.BaseDirectory, configured));
    }

    private static List<RunSummary> LoadHistory(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<RunSummary>>(File.ReadAllText(path), JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return []; // Corrupt history should never break the run.
        }
    }

    private static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("DemoApp.UiTests.Reporting.report-template.html")
            ?? throw new InvalidOperationException("Embedded report template not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static void TryOpen(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // No desktop/browser available (e.g. CI); the file is still written.
        }
    }

    public sealed record RunSummary
    {
        public DateTimeOffset StartedAt { get; init; }

        public DateTimeOffset FinishedAt { get; init; }

        public double DurationSeconds { get; init; }

        public int Total { get; init; }

        public int Passed { get; init; }

        public int Failed { get; init; }

        public int Skipped { get; init; }

        public string Browser { get; init; } = string.Empty;

        public bool Headless { get; init; }

        public string BaseUrl { get; init; } = string.Empty;

        public string Machine { get; init; } = string.Empty;

        public string Os { get; init; } = string.Empty;
    }
}
