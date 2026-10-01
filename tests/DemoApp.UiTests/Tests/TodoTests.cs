using DemoApp.UiTests.Pages;
using DemoApp.UiTests.TestData;

namespace DemoApp.UiTests.Tests;

[TestFixture]
[Category("Todos")]
[Parallelizable(ParallelScope.All)]
public sealed class TodoTests : BaseTest
{
    private DashboardPage _dashboard = null!;

    [SetUp]
    public void LogIn() =>
        _dashboard = new LoginPage(Driver, BaseUrl).Open()
            .LoginAs(Users.Standard.Username, Users.Standard.Password);

    [Test]
    [Category("Smoke")]
    public void AddTodo_AppearsInList_AndCounterIncrements()
    {
        _dashboard.AddTodo("Write Selenium tests");

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.TodoTitles, Is.EqualTo(new[] { "Write Selenium tests" }));
            Assert.That(_dashboard.ItemsLeft, Is.EqualTo(1));
        });
    }

    [Test]
    public void AddMultipleTodos_PreservesOrder()
    {
        string[] titles = ["Plan", "Build", "Test", "Ship"];

        _dashboard.AddTodos(titles);

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.TodoTitles, Is.EqualTo(titles));
            Assert.That(_dashboard.ItemsLeft, Is.EqualTo(titles.Length));
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void BlankTodo_IsNotAdded(string title)
    {
        _dashboard.SubmitTodo(title);

        Assert.That(_dashboard.TodoTitles, Is.Empty);
    }

    [Test]
    public void CompleteTodo_StrikesThrough_AndDecrementsCounter()
    {
        _dashboard.AddTodos("Buy milk", "Walk dog").ToggleTodo("Buy milk");

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.IsCompleted("Buy milk"), Is.True);
            Assert.That(_dashboard.IsCompleted("Walk dog"), Is.False);
            Assert.That(_dashboard.ItemsLeft, Is.EqualTo(1));
        });
    }

    [Test]
    public void DeleteTodo_RemovesItFromList()
    {
        _dashboard.AddTodos("Keep me", "Delete me").DeleteTodo("Delete me");

        Assert.That(_dashboard.TodoTitles, Is.EqualTo(new[] { "Keep me" }));
    }

    [Test]
    public void Filters_ShowOnlyMatchingTodos()
    {
        _dashboard.AddTodos("Done task", "Open task").ToggleTodo("Done task");

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.FilterBy(TodoFilter.Active).TodoTitles, Is.EqualTo(new[] { "Open task" }));
            Assert.That(_dashboard.FilterBy(TodoFilter.Completed).TodoTitles, Is.EqualTo(new[] { "Done task" }));
            Assert.That(_dashboard.FilterBy(TodoFilter.All).TodoTitles, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Todos_ArePreserved_AfterPageRefresh()
    {
        _dashboard.AddTodo("Survive refresh");

        Driver.Navigate().Refresh();
        _dashboard.WaitUntilLoaded();

        Assert.That(_dashboard.TodoTitles, Is.EqualTo(new[] { "Survive refresh" }));
    }
}
