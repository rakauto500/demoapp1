using System.Collections.Concurrent;

namespace DemoApp.UiTests.Reporting;

/// <summary>Thread-safe collector for results of the current run (tests run in parallel).</summary>
public static class TestRunRecorder
{
    private static readonly ConcurrentQueue<TestResultRecord> Results = new();

    public static DateTimeOffset StartedAt { get; private set; } = DateTimeOffset.Now;

    public static void Start()
    {
        Results.Clear();
        StartedAt = DateTimeOffset.Now;
    }

    public static void Add(TestResultRecord result) => Results.Enqueue(result);

    public static IReadOnlyList<TestResultRecord> Snapshot() => Results.ToArray();
}
