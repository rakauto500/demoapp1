using DemoApp.UiTests.Config;
using DemoApp.UiTests.Drivers;
using NUnit.Framework.Interfaces;

namespace DemoApp.UiTests.Tests;

/// <summary>
/// Base class for every UI test: a fresh browser per test (full isolation,
/// safe for parallel execution) plus a screenshot and page source on failure.
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class BaseTest
{
    private IWebDriver? _driver;

    protected IWebDriver Driver => _driver ?? throw new InvalidOperationException("Browser was not started.");

    protected static string BaseUrl => TestEnvironment.BaseUrl;

    [SetUp]
    public void StartBrowser() => _driver = WebDriverFactory.Create(TestSettings.Current);

    [TearDown]
    public void StopBrowser()
    {
        if (_driver is null)
        {
            return; // Browser failed to start; the SetUp error is the real failure.
        }

        try
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                CaptureFailureArtifacts();
            }
        }
        finally
        {
            _driver.Quit();
            _driver.Dispose();
            _driver = null;
        }
    }

    private void CaptureFailureArtifacts()
    {
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, TestSettings.Current.ArtifactsDirectory);
        Directory.CreateDirectory(directory);
        var name = string.Concat(TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));

        try
        {
            var screenshotPath = Path.Combine(directory, $"{name}.png");
            ((ITakesScreenshot)Driver).GetScreenshot().SaveAsFile(screenshotPath);
            TestContext.AddTestAttachment(screenshotPath, "Screenshot at failure");

            var sourcePath = Path.Combine(directory, $"{name}.html");
            File.WriteAllText(sourcePath, Driver.PageSource);
            TestContext.AddTestAttachment(sourcePath, "Page source at failure");
        }
        catch (WebDriverException ex)
        {
            TestContext.Progress.WriteLine($"Could not capture failure artifacts: {ex.Message}");
        }
    }
}
