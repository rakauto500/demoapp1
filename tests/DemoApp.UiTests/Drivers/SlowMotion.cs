using OpenQA.Selenium.Support.Events;

namespace DemoApp.UiTests.Drivers;

/// <summary>
/// Slow-motion mode for demos and debugging. Wraps the driver in Selenium's
/// <see cref="EventFiringWebDriver"/> so every click, keystroke and navigation —
/// including ones made directly on elements — pauses and highlights its target,
/// without touching page objects or tests.
/// </summary>
public static class SlowMotion
{
    private const string Highlight =
        "arguments[0].style.outline='3px solid #e34948'; arguments[0].style.outlineOffset='2px';";

    private const string Unhighlight =
        "arguments[0].style.outline=''; arguments[0].style.outlineOffset='';";

    public static IWebDriver Wrap(IWebDriver driver, int delayMs)
    {
        if (delayMs <= 0)
        {
            return driver;
        }

        var delay = TimeSpan.FromMilliseconds(delayMs);
        var firing = new EventFiringWebDriver(driver);

        firing.ElementClicking += (_, e) => HighlightAndPause(e, delay);
        firing.ElementClicked += (_, e) => RemoveHighlight(e);
        firing.ElementValueChanging += (_, e) => HighlightAndPause(e, delay);
        firing.ElementValueChanged += (_, e) => RemoveHighlight(e);
        firing.Navigated += (_, _) => Pause(delay);

        return firing;
    }

    /// <summary>Lets the viewer see the final state before the browser closes.</summary>
    public static void PauseAtEnd(int delayMs)
    {
        if (delayMs > 0)
        {
            Pause(TimeSpan.FromMilliseconds(delayMs * 2));
        }
    }

    private static void HighlightAndPause(WebElementEventArgs e, TimeSpan delay)
    {
        TryScript(e, Highlight);
        Pause(delay);
    }

    private static void RemoveHighlight(WebElementEventArgs e) => TryScript(e, Unhighlight);

    private static void TryScript(WebElementEventArgs e, string script)
    {
        try
        {
            ((IJavaScriptExecutor)e.Driver).ExecuteScript(script, e.Element);
        }
        catch (WebDriverException)
        {
            // Element may have gone stale (e.g. the click navigated away); the highlight is cosmetic.
        }
    }

    // Deliberate, opt-in visual delay (SlowMoMs > 0 only). This is the single sanctioned
    // sleep in the framework; synchronisation always uses explicit waits.
    private static void Pause(TimeSpan delay) => Thread.Sleep(delay);
}
