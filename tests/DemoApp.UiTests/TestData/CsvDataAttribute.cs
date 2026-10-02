namespace DemoApp.UiTests.TestData;

/// <summary>
/// Runs the test once per row of a CSV file, passing the row as a typed <typeparamref name="TCase"/>.
/// Usage: <c>[CsvData&lt;InvalidLoginCase&gt;("login_invalid.csv")]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class CsvDataAttribute<TCase> : TestCaseSourceAttribute
    where TCase : CsvCase
{
    public CsvDataAttribute(string fileName)
        : base(typeof(CsvData), nameof(CsvData.TestCases), [fileName, typeof(TCase)])
    {
    }
}
