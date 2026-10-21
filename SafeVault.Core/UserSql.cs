namespace SafeVault.Core;

public static class UserSql
{
    public static ParameterizedSql FindByEmail(string email) => new(
        "SELECT UserId, Username, Email FROM Users WHERE Email = @Email;",
        new Dictionary<string, object> { ["@Email"] = email });

    public static ParameterizedSql Insert(UserInput user) => new(
        "INSERT INTO Users (Username, Email) VALUES (@Username, @Email);",
        new Dictionary<string, object>
        {
            ["@Username"] = user.Username,
            ["@Email"] = user.Email
        });
}
