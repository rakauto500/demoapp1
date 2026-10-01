using System.Diagnostics;
using System.Reflection;
using DemoApp.UiTests.Config;
using DemoApp.UiTests.Drivers;
using DemoApp.UiTests.Reporting;
using NUnit.Framework.Interfaces;

namespace DemoApp.UiTests.Tests;

/// <summary>
/// Base class for every UI test: a fresh browser per test (full isolation,
/// safe for parallel execution), a screenshot and page source on failure,
/// and a result record for the HTML dashboard.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class BaseTest
{
    private readonly Stopwatch _stopwatch = new();
    private DateTimeOffset _startedAt;
    private IWebDriver? _driver;

    protected IWebDriver Driver => _driver ?? throw new InvalidOperationException("Browser was not started.");

    protected static string BaseUrl => TestEnvironment.BaseUrl;

    [SetUp]
    public void StartBrowser()
    {
        _startedAt = DateTimeOffset.Now;
        _stopwatch.Start();
        _driver = WebDriverFactory.Create(TestSettings.Current);
    }

    [TearDown]
    public void StopBrowser()
    {
        var result = TestContext.CurrentContext.Result;
        string? screenshot = null;

        try
        {
            if (_driver is not null)
            {
                if (result.Outcome.Status == TestStatus.Failed)
                {
                    screenshot = CaptureFailureArtifacts(_driver);
                }

                SlowMotion.PauseAtEnd(TestSettings.Current.SlowMoMs);
            }
        }
        finally
        {
            _driver?.Quit();
            _driver?.Dispose();
            _driver = null;
            _stopwatch.Stop();
            Record(result, screenshot);
        }
    }

    private void Record(TestContext.ResultAdapter result, string? screenshot)
    {
        var test = TestContext.CurrentContext.Test;
        var categories = GetType().GetCustomAttributes<CategoryAttribute>(inherit: true).Select(c => c.Name)
            .Concat(test.Properties["Category"].Select(c => c?.ToString() ?? string.Empty))
            .Where(c => c.Length > 0)
            .Distinct()
            .ToList();

        TestRunRecorder.Add(new TestResultRecord
        {
            Name = test.Name,
            FullName = test.FullName,
            Fixture = GetType().Name,
            Categories = categories,
            Outcome = result.Outcome.Status.ToString(),
            DurationSeconds = Math.Round(_stopwatch.Elapsed.TotalSeconds, 3),
            StartedAt = _startedAt,
            Message = string.IsNullOrWhiteSpace(result.Message) ? null : result.Message.Trim(),
            StackTrace = string.IsNullOrWhiteSpace(result.StackTrace) ? null : result.StackTrace.Trim(),
            ScreenshotBase64 = screenshot,
        });
    }

    /// <returns>The screenshot as base64 (for the HTML report), or null if capture failed.</returns>
    private static string? CaptureFailureArtifacts(IWebDriver driver)
    {
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestSettings.Current.ArtifactsDirectory);
        Directory.CreateDirectory(directory);
        var name = string.Concat(TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));

        try
        {
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            var screenshotPath = Path.Combine(directory, $"{name}.png");
            screenshot.SaveAsFile(screenshotPath);
            TestContext.AddTestAttachment(screenshotPath, "Screenshot at failure");

            var sourcePath = Path.Combine(directory, $"{name}.html");
            File.WriteAllText(sourcePath, driver.PageSource);
            TestContext.AddTestAttachment(sourcePath, "Page source at failure");

            return screenshot.AsBase64EncodedString;
        }
        catch (WebDriverException ex)
        {
            TestContext.Progress.WriteLine($"Could not capture failure artifacts: {ex.Message}");
            return null;
        }
    }
}
