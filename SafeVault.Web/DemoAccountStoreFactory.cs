using Microsoft.AspNetCore.Identity;
using SafeVault.Core.Authentication;

namespace SafeVault.Web;

public static class DemoAccountStoreFactory
{
    public static InMemoryUserAccountStore Create(
        IConfiguration configuration,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        var adminUsername = configuration["SAFEVAULT_ADMIN_USERNAME"] ?? "admin";
        var userUsername = configuration["SAFEVAULT_USER_USERNAME"] ?? "user";
        var adminPassword = RequirePassword(configuration["SAFEVAULT_ADMIN_PASSWORD"], "SAFEVAULT_ADMIN_PASSWORD");
        var userPassword = RequirePassword(configuration["SAFEVAULT_USER_PASSWORD"], "SAFEVAULT_USER_PASSWORD");

        return new InMemoryUserAccountStore(new[]
        {
            CreateAccount(adminUsername, adminPassword, "admin", passwordHasher),
            CreateAccount(userUsername, userPassword, "user", passwordHasher)
        });
    }

    private static UserAccount CreateAccount(
        string username,
        string password,
        string role,
        IPasswordHasher<UserAccount> passwordHasher)
    {
        var account = new UserAccount(username, string.Empty, role);
        return account with { PasswordHash = passwordHasher.HashPassword(account, password) };
    }

    private static string RequirePassword(string? password, string settingName)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            throw new InvalidOperationException($"Set {settingName} to a password of at least 12 characters before starting SafeVault.Web.");
        }

        return password;
    }
}
