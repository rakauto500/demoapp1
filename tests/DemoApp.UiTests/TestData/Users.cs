namespace DemoApp.UiTests.TestData;

/// <summary>Named accounts from users.csv (column Role), for tests that only need a logged-in user.</summary>
public static class Users
{
    public static UserRecord Standard => Get("Standard");

    public static UserRecord Get(string role) =>
        CsvData.Load<UserRecord>("users.csv").FirstOrDefault(u => string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidDataException($"No user with Role '{role}' in users.csv.");
}
