using DemoApp.UiTests.Config;
using OpenQA.Selenium.Support.UI;

namespace DemoApp.UiTests.Pages;

/// <summary>
/// Common page behaviour. All element interaction goes through explicit waits so
/// tests never depend on Thread.Sleep or implicit waits.
/// </summary>
public abstract class BasePage
{
    protected BasePage(IWebDriver driver, string baseUrl)
    {
        Driver = driver;
        BaseUrl = baseUrl;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestSettings.Current.ExplicitWaitSeconds));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
    }

    protected IWebDriver Driver { get; }

    protected string BaseUrl { get; }

    protected WebDriverWait Wait { get; }

    /// <summary>Relative path of the page, e.g. "index.html".</summary>
    protected abstract string Path { get; }

    public string Title => Driver.Title;

    public string CurrentUrl => Driver.Url;

    /// <summary>Default readiness check: the page URL matches <see cref="Path"/>.</summary>
    public virtual bool IsLoaded => Driver.Url.Contains(Path, StringComparison.OrdinalIgnoreCase);

    protected static By TestId(string id) => By.CssSelector($"[data-test='{id}']");

    protected void NavigateTo() => Driver.Navigate().GoToUrl(BaseUrl + Path);

    protected IWebElement WaitVisible(By locator) =>
        Wait.Until(d =>
        {
            var element = d.FindElement(locator);
            return element.Displayed ? element : null;
        })!;

    protected IWebElement WaitClickable(By locator) =>
        Wait.Until(d =>
        {
            var element = d.FindElement(locator);
            return element.Displayed && element.Enabled ? element : null;
        })!;

    protected void Click(By locator) => WaitClickable(locator).Click();

    protected void Type(By locator, string text)
    {
        var element = WaitVisible(locator);
        element.Clear();
        element.SendKeys(text);
    }

    protected string TextOf(By locator) => WaitVisible(locator).Text;

    protected bool IsDisplayed(By locator)
    {
        var elements = Driver.FindElements(locator);
        return elements.Count > 0 && elements[0].Displayed;
    }

    protected void WaitForUrlContains(string fragment) =>
        Wait.Until(d => d.Url.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}
