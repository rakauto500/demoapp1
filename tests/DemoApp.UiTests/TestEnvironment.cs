using DemoApp.UiTests.Config;
using DemoApp.UiTests.Infrastructure;

namespace DemoApp.UiTests;

/// <summary>
/// Root-namespace setup (NUnit scopes [SetUpFixture] to its namespace, so this must
/// live in DemoApp.UiTests to cover every test). Assembly-level setup: starts the embedded web server once for the whole run
/// (unless BaseUrl points at an external environment) and tears it down at the end.
/// </summary>
[SetUpFixture]
public sealed class TestEnvironment
{
    private static StaticFileServer? _server;

    public static string BaseUrl { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public void StartEnvironment()
    {
        var settings = TestSettings.Current;
        if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            BaseUrl = settings.BaseUrl.EndsWith('/') ? settings.BaseUrl : settings.BaseUrl + "/";
            return;
        }

        _server = new StaticFileServer(Path.Combine(AppContext.BaseDirectory, "wwwroot"));
        _server.Start();
        BaseUrl = _server.BaseUrl;
        TestContext.Progress.WriteLine($"Serving demo app at {BaseUrl}");
    }

    [OneTimeTearDown]
    public void StopEnvironment()
    {
        _server?.Dispose();
        _server = null;
    }
}
