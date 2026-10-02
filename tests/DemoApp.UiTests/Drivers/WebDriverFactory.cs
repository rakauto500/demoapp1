using System.Drawing;
using DemoApp.UiTests.Config;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace DemoApp.UiTests.Drivers;

/// <summary>Creates configured WebDriver instances. One driver per test.</summary>
public static class WebDriverFactory
{
    public static IWebDriver Create(TestSettings settings)
    {
        IWebDriver driver = settings.Browser switch
        {
            BrowserType.Chrome => CreateChrome(settings),
            BrowserType.Edge => CreateEdge(settings),
            BrowserType.Firefox => CreateFirefox(settings),
            _ => throw new NotSupportedException($"Browser '{settings.Browser}' is not supported."),
        };

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(settings.ImplicitWaitSeconds);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(settings.PageLoadTimeoutSeconds);
        driver.Manage().Window.Size = new Size(settings.WindowWidth, settings.WindowHeight);
        return SlowMotion.Wrap(driver, settings.SlowMoMs);
    }

    private static IWebDriver CreateChrome(TestSettings settings)
    {
        var options = new ChromeOptions();
        AddChromiumArguments(options, settings);
        if (!string.IsNullOrWhiteSpace(settings.BrowserBinaryPath))
        {
            options.BinaryLocation = settings.BrowserBinaryPath;
        }

        return string.IsNullOrWhiteSpace(settings.DriverPath)
            ? new ChromeDriver(options)
            : new ChromeDriver(ChromeDriverService.CreateDefaultService(settings.DriverPath), options);
    }

    private static IWebDriver CreateEdge(TestSettings settings)
    {
        var options = new EdgeOptions();
        AddChromiumArguments(options, settings);
        if (!string.IsNullOrWhiteSpace(settings.BrowserBinaryPath))
        {
            options.BinaryLocation = settings.BrowserBinaryPath;
        }

        return string.IsNullOrWhiteSpace(settings.DriverPath)
            ? new EdgeDriver(options)
            : new EdgeDriver(EdgeDriverService.CreateDefaultService(settings.DriverPath), options);
    }

    private static IWebDriver CreateFirefox(TestSettings settings)
    {
        var options = new FirefoxOptions();
        if (settings.Headless)
        {
            options.AddArgument("-headless");
        }

        if (!string.IsNullOrWhiteSpace(settings.BrowserBinaryPath))
        {
            options.BinaryLocation = settings.BrowserBinaryPath;
        }

        return string.IsNullOrWhiteSpace(settings.DriverPath)
            ? new FirefoxDriver(options)
            : new FirefoxDriver(FirefoxDriverService.CreateDefaultService(settings.DriverPath), options);
    }

    private static void AddChromiumArguments(OpenQA.Selenium.Chromium.ChromiumOptions options, TestSettings settings)
    {
        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
        }

        // Required for containers / CI runners without a user namespace sandbox.
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
    }
}
