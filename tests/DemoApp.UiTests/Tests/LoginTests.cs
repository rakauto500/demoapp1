using DemoApp.UiTests.Pages;
using DemoApp.UiTests.TestData;

namespace DemoApp.UiTests.Tests;

[TestFixture]
[Category("Login")]
[Parallelizable(ParallelScope.All)]
public sealed class LoginTests : BaseTest
{
    [Test]
    [Category("Smoke")]
    public void ValidUser_CanLogIn_AndSeesWelcomeMessage()
    {
        var dashboard = new LoginPage(Driver, BaseUrl).Open()
            .LoginAs(Users.Standard.Username, Users.Standard.Password);

        Assert.Multiple(() =>
        {
            Assert.That(dashboard.IsLoaded, Is.True, "Dashboard should be displayed after login");
            Assert.That(dashboard.WelcomeUserName, Is.EqualTo(Users.Standard.Username));
            Assert.That(dashboard.Title, Is.EqualTo("DemoApp - Dashboard"));
        });
    }

    [TestCase("demo", "wrong-password", "Invalid username or password", TestName = "WrongPassword_ShowsError")]
    [TestCase("nobody", "secret123", "Invalid username or password", TestName = "UnknownUser_ShowsError")]
    [TestCase("locked", "secret123", "This account has been locked", TestName = "LockedUser_ShowsError")]
    [TestCase("", "secret123", "Username is required", TestName = "EmptyUsername_ShowsError")]
    [TestCase("demo", "", "Password is required", TestName = "EmptyPassword_ShowsError")]
    public void InvalidLogin_ShowsExpectedError(string username, string password, string expectedError)
    {
        var login = new LoginPage(Driver, BaseUrl).Open().AttemptLogin(username, password);

        Assert.Multiple(() =>
        {
            Assert.That(login.ErrorMessage, Is.EqualTo(expectedError));
            Assert.That(login.IsLoaded, Is.True, "User should remain on the login page");
        });
    }

    [Test]
    public void Logout_ReturnsToLoginPage()
    {
        var login = new LoginPage(Driver, BaseUrl).Open()
            .LoginAs(Users.Standard.Username, Users.Standard.Password)
            .Logout();

        Assert.That(login.IsLoaded, Is.True);
    }

    [Test]
    [Category("Security")]
    public void Dashboard_WithoutSession_RedirectsToLogin()
    {
        new DashboardPage(Driver, BaseUrl).OpenDirectly();
        var login = new LoginPage(Driver, BaseUrl);

        Assert.That(() => login.IsLoaded, Is.True.After(5).Seconds.PollEvery(200).MilliSeconds);
    }
}
