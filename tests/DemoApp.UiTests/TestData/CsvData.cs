using System.Collections.Concurrent;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DemoApp.UiTests.Config;

namespace DemoApp.UiTests.TestData;

/// <summary>
/// Loads test data from CSV files in the test-data folder (default: TestData next to the
/// test binaries; override with Test:TestDataDirectory). Files are read once and cached.
/// </summary>
public static class CsvData
{
    private static readonly ConcurrentDictionary<(string File, Type Type), IReadOnlyList<object>> Cache = new();

    public static string Directory
    {
        get
        {
            var configured = TestSettings.Current.TestDataDirectory;
            return Path.IsPathRooted(configured) ? configured : Path.Combine(AppContext.BaseDirectory, configured);
        }
    }

    public static IReadOnlyList<T> Load<T>(string fileName) => Load(fileName, typeof(T)).Cast<T>().ToList();

    public static IReadOnlyList<object> Load(string fileName, Type recordType) =>
        Cache.GetOrAdd((fileName, recordType), key => Read(key.File, key.Type));

    /// <summary>NUnit test-case source used by <see cref="CsvDataAttribute{TCase}"/>: one test case per row.</summary>
    public static IEnumerable<TestCaseData> TestCases(string fileName, Type caseType)
    {
        foreach (CsvCase row in Load(fileName, caseType))
        {
            var testCase = new TestCaseData(row).SetArgDisplayNames(row.CaseName);
            foreach (var category in row.Categories.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                testCase.SetCategory(category);
            }

            yield return testCase;
        }
    }

    private static List<object> Read(string fileName, Type recordType)
    {
        var path = Path.Combine(Directory, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Test data file not found: {path}", path);
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
            TrimOptions = TrimOptions.None, // keep deliberate whitespace, e.g. a "   " title
            AllowComments = true,           // lines starting with # are notes for humans
            Comment = '#',
        };

        try
        {
            using var reader = new StreamReader(path); // detects a UTF-8 BOM (Excel "CSV UTF-8")
            using var csv = new CsvReader(reader, config);
            var rows = csv.GetRecords(recordType).ToList();
            Validate(path, rows);
            return rows;
        }
        catch (CsvHelperException ex)
        {
            // Keep CsvHelper's first lines (what is wrong + the headers found); drop its developer advice.
            var message = ex.Message.Split("If you are expecting", 2)[0].Trim();
            throw new InvalidDataException($"Invalid test data in {path}: {message}", ex);
        }
    }

    private static void Validate(string path, List<object> rows)
    {
        if (rows.Count == 0)
        {
            throw new InvalidDataException($"Test data file has no rows: {path}");
        }

        var cases = rows.OfType<CsvCase>().ToList();
        var blank = cases.FindIndex(c => string.IsNullOrWhiteSpace(c.CaseName));
        if (blank >= 0)
        {
            throw new InvalidDataException($"Row {blank + 1} in {path} has an empty CaseName.");
        }

        var duplicate = cases.GroupBy(c => c.CaseName, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidDataException($"Duplicate CaseName '{duplicate.Key}' in {path}.");
        }
    }
}
