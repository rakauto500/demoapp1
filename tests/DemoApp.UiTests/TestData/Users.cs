namespace DemoApp.UiTests.TestData;

public sealed record UserCredentials(string Username, string Password);

/// <summary>Known accounts of the demo app. Keep test data out of test bodies.</summary>
public static class Users
{
    public static readonly UserCredentials Standard = new("demo", "secret123");
    public static readonly UserCredentials Locked = new("locked", "secret123");
    public static readonly UserCredentials WrongPassword = new("demo", "wrong-password");
    public static readonly UserCredentials Unknown = new("nobody", "secret123");
}
