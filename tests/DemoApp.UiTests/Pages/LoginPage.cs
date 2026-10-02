namespace DemoApp.UiTests.Pages;

public sealed class LoginPage : BasePage
{
    private static readonly By Username = TestId("username");
    private static readonly By Password = TestId("password");
    private static readonly By LoginButton = TestId("login-button");
    private static readonly By Error = TestId("error");

    public LoginPage(IWebDriver driver, string baseUrl)
        : base(driver, baseUrl)
    {
    }

    protected override string Path => "index.html";

    public override bool IsLoaded => IsDisplayed(LoginButton);

    public LoginPage Open()
    {
        NavigateTo();
        WaitVisible(LoginButton);
        return this;
    }

    public LoginPage EnterUsername(string username)
    {
        Type(Username, username);
        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        Type(Password, password);
        return this;
    }

    public void Submit() => Click(LoginButton);

    /// <summary>Happy path: log in and land on the dashboard.</summary>
    public DashboardPage LoginAs(string username, string password)
    {
        EnterUsername(username).EnterPassword(password).Submit();
        return new DashboardPage(Driver, BaseUrl).WaitUntilLoaded();
    }

    /// <summary>Negative path: attempt to log in and stay on the login page.</summary>
    public LoginPage AttemptLogin(string username, string password)
    {
        EnterUsername(username).EnterPassword(password).Submit();
        WaitVisible(Error);
        return this;
    }

    public string ErrorMessage => TextOf(Error);

    public bool IsErrorDisplayed => IsDisplayed(Error);
}
