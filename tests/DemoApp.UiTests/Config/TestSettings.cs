using Microsoft.Extensions.Configuration;

namespace DemoApp.UiTests.Config;

public enum BrowserType
{
    Chrome,
    Firefox,
    Edge,
}

/// <summary>
/// Strongly typed test settings. Values come from appsettings.json and can be
/// overridden with environment variables prefixed with UITEST_, e.g.
/// UITEST_Test__Headless=false or UITEST_Test__BaseUrl=https://staging.example.com.
/// </summary>
public sealed class TestSettings
{
    public BrowserType Browser { get; init; } = BrowserType.Chrome;
    public bool Headless { get; init; } = true;

    /// <summary>When empty, tests run against the embedded server hosting ./app.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    public int ImplicitWaitSeconds { get; init; }
    public int ExplicitWaitSeconds { get; init; } = 10;
    public int PageLoadTimeoutSeconds { get; init; } = 30;
    public int WindowWidth { get; init; } = 1366;
    public int WindowHeight { get; init; } = 900;

    /// <summary>Optional browser executable; empty lets Selenium Manager resolve one.</summary>
    public string BrowserBinaryPath { get; init; } = string.Empty;

    /// <summary>Optional driver executable; empty lets Selenium Manager resolve one.</summary>
    public string DriverPath { get; init; } = string.Empty;

    public string ArtifactsDirectory { get; init; } = "TestResults/artifacts";

    /// <summary>
    /// Folder with the scenario CSV files. Relative paths resolve from the test binaries
    /// folder (the files are copied there on build); absolute paths allow an external data set.
    /// </summary>
    public string TestDataDirectory { get; init; } = "TestData/Csv";

    /// <summary>
    /// Visual aid for watching a run: milliseconds to pause (with the target element
    /// highlighted) before every click, keystroke and after every navigation.
    /// 0 = off. Never enable in CI.
    /// </summary>
    public int SlowMoMs { get; init; }

    /// <summary>
    /// Folder for the HTML dashboard (index.html + history.json). Relative paths are
    /// resolved from the repository root (the folder containing the .sln).
    /// </summary>
    public string ReportDirectory { get; init; } = "TestResults/report";

    /// <summary>Open the HTML dashboard in the default browser when the run finishes.</summary>
    public bool OpenReport { get; init; }

    private static readonly Lazy<TestSettings> Instance = new(Load);

    public static TestSettings Current => Instance.Value;

    private static TestSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables(prefix: "UITEST_")
            .Build();

        return configuration.GetSection("Test").Get<TestSettings>() ?? new TestSettings();
    }
}
