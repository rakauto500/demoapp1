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
    public void LogIn()
    {
        var user = Users.Standard;
        _dashboard = new LoginPage(Driver, BaseUrl).Open().LoginAs(user.Username, user.Password);
    }

    [CsvData<AddTodoCase>("todo_add.csv")]
    public void AddTodos_AppearInOrder_AndCounterMatches(AddTodoCase data)
    {
        _dashboard.AddTodos([.. data.TodoList]);

        Assert.Multiple(() =>
        {
            Assert.That(_dashboard.TodoTitles, Is.EqualTo(data.TodoList));
            Assert.That(_dashboard.ItemsLeft, Is.EqualTo(data.ExpectedItemsLeft));
        });
    }

    [CsvData<BlankTodoCase>("todo_blank.csv")]
    public void BlankTodo_IsNotAdded(BlankTodoCase data)
    {
        _dashboard.SubmitTodo(data.Todo);

        Assert.That(_dashboard.TodoTitles, Is.Empty);
    }

    [CsvData<CompleteTodoCase>("todo_complete.csv")]
    public void CompleteTodos_StrikeThrough_AndCounterDecrements(CompleteTodoCase data)
    {
        _dashboard.AddTodos([.. data.TodoList]);
        foreach (var title in data.CompleteList)
        {
            _dashboard.ToggleTodo(title);
        }

        Assert.Multiple(() =>
        {
            foreach (var title in data.TodoList)
            {
                Assert.That(_dashboard.IsCompleted(title), Is.EqualTo(data.CompleteList.Contains(title)), $"Completed state of '{title}'");
            }

            Assert.That(_dashboard.ItemsLeft, Is.EqualTo(data.ExpectedItemsLeft));
        });
    }

    [CsvData<DeleteTodoCase>("todo_delete.csv")]
    public void DeleteTodo_RemovesOnlyThatItem(DeleteTodoCase data)
    {
        _dashboard.AddTodos([.. data.TodoList]).DeleteTodo(data.Delete);

        Assert.That(_dashboard.TodoTitles, Is.EqualTo(data.ExpectedRemainingList));
    }

    [CsvData<FilterTodoCase>("todo_filter.csv")]
    public void Filter_ShowsOnlyMatchingTodos(FilterTodoCase data)
    {
        _dashboard.AddTodos([.. data.TodoList]);
        foreach (var title in data.CompleteList)
        {
            _dashboard.ToggleTodo(title);
        }

        Assert.That(_dashboard.FilterBy(data.Filter).TodoTitles, Is.EqualTo(data.ExpectedVisibleList));
    }

    [CsvData<RefreshTodoCase>("todo_refresh.csv")]
    public void Todos_ArePreserved_AfterPageRefresh(RefreshTodoCase data)
    {
        _dashboard.AddTodos([.. data.TodoList]);

        Driver.Navigate().Refresh();
        _dashboard.WaitUntilLoaded();

        Assert.That(_dashboard.TodoTitles, Is.EqualTo(data.TodoList));
    }
}
