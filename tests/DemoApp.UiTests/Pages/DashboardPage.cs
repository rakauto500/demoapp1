using System.Collections.ObjectModel;

namespace DemoApp.UiTests.Pages;

public sealed class DashboardPage : BasePage
{
    private static readonly By WelcomeUser = TestId("welcome-user");
    private static readonly By LogoutButton = TestId("logout-button");
    private static readonly By NewTodoInput = TestId("new-todo");
    private static readonly By AddTodoButton = TestId("add-todo");
    private static readonly By TodoItems = TestId("todo-item");
    private static readonly By TodoCount = TestId("todo-count");

    public DashboardPage(IWebDriver driver, string baseUrl)
        : base(driver, baseUrl)
    {
    }

    protected override string Path => "dashboard.html";

    public override bool IsLoaded => base.IsLoaded && IsDisplayed(LogoutButton);

    /// <summary>Navigates directly to the dashboard (used for access-control tests).</summary>
    public void OpenDirectly() => NavigateTo();

    public DashboardPage WaitUntilLoaded()
    {
        WaitForUrlContains(Path);
        WaitVisible(LogoutButton);
        return this;
    }

    public string WelcomeUserName => TextOf(WelcomeUser);

    public int ItemsLeft => int.Parse(TextOf(TodoCount), System.Globalization.CultureInfo.InvariantCulture);

    public DashboardPage AddTodo(string title)
    {
        var expected = Todos.Count + 1;
        Type(NewTodoInput, title);
        Click(AddTodoButton);
        Wait.Until(_ => Todos.Count == expected);
        return this;
    }

    public DashboardPage AddTodos(params string[] titles)
    {
        foreach (var title in titles)
        {
            AddTodo(title);
        }

        return this;
    }

    /// <summary>Clicks Add with whatever is currently in the input (may be empty).</summary>
    public DashboardPage SubmitTodo(string title)
    {
        Type(NewTodoInput, title);
        Click(AddTodoButton);
        return this;
    }

    public IReadOnlyList<string> TodoTitles =>
        Todos.Select(item => item.FindElement(By.CssSelector(".title")).Text).ToList();

    public DashboardPage ToggleTodo(string title)
    {
        FindTodo(title).FindElement(By.CssSelector(".toggle")).Click();
        return this;
    }

    public DashboardPage DeleteTodo(string title)
    {
        var expected = Todos.Count - 1;
        FindTodo(title).FindElement(By.CssSelector(".delete")).Click();
        Wait.Until(_ => Todos.Count == expected);
        return this;
    }

    public bool IsCompleted(string title) =>
        FindTodo(title).GetAttribute("class")?.Contains("completed", StringComparison.Ordinal) == true;

    public DashboardPage FilterBy(TodoFilter filter)
    {
        Click(By.CssSelector($"[data-filter='{filter.ToString().ToLowerInvariant()}']"));
        Wait.Until(d => d.FindElement(By.CssSelector($"[data-filter='{filter.ToString().ToLowerInvariant()}']"))
            .GetAttribute("class")?.Contains("active", StringComparison.Ordinal) == true);
        return this;
    }

    public LoginPage Logout()
    {
        Click(LogoutButton);
        var login = new LoginPage(Driver, BaseUrl);
        Wait.Until(_ => login.IsLoaded);
        return login;
    }

    private ReadOnlyCollection<IWebElement> Todos => Driver.FindElements(TodoItems);

    private IWebElement FindTodo(string title) =>
        Wait.Until(_ => Todos.FirstOrDefault(item => item.FindElement(By.CssSelector(".title")).Text == title))
        ?? throw new NoSuchElementException($"Todo '{title}' not found.");
}

public enum TodoFilter
{
    All,
    Active,
    Completed,
}
