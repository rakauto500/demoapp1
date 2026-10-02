using DemoApp.UiTests.Pages;
using DemoApp.UiTests.TestData;

namespace DemoApp.UiTests.Tests;

[TestFixture]
[Category("Login")]
[Parallelizable(ParallelScope.All)]
public sealed class LoginTests : BaseTest
{
    [CsvData<ValidLoginCase>("login_valid.csv")]
    public void ValidLogin_ShowsDashboard(ValidLoginCase data)
    {
        var dashboard = new LoginPage(Driver, BaseUrl).Open().LoginAs(data.Username, data.Password);

        Assert.Multiple(() =>
        {
            Assert.That(dashboard.IsLoaded, Is.True, "Dashboard should be displayed after login");
            Assert.That(dashboard.WelcomeUserName, Is.EqualTo(data.ExpectedWelcomeName));
            Assert.That(dashboard.Title, Is.EqualTo(data.ExpectedTitle));
        });
    }

    [CsvData<InvalidLoginCase>("login_invalid.csv")]
    public void InvalidLogin_ShowsExpectedError(InvalidLoginCase data)
    {
        var login = new LoginPage(Driver, BaseUrl).Open().AttemptLogin(data.Username, data.Password);

        Assert.Multiple(() =>
        {
            Assert.That(login.ErrorMessage, Is.EqualTo(data.ExpectedError));
            Assert.That(login.IsLoaded, Is.True, "User should remain on the login page");
        });
    }

    [Test]
    public void Logout_ReturnsToLoginPage()
    {
        var user = Users.Standard;
        var login = new LoginPage(Driver, BaseUrl).Open()
            .LoginAs(user.Username, user.Password)
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
