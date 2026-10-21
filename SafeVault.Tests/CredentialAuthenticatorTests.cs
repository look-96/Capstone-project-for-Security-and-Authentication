using Microsoft.AspNetCore.Identity;
using SafeVault.Core.Authentication;

namespace SafeVault.Tests;

[TestFixture]
public sealed class CredentialAuthenticatorTests
{
    private CredentialAuthenticator _authenticator = null!;

    [SetUp]
    public void SetUp()
    {
        var passwordHasher = new PasswordHasher<UserAccount>();
        var admin = CreateAccount("admin", "Admin-password-123!", "admin", passwordHasher);
        var user = CreateAccount("sam", "User-password-123!", "user", passwordHasher);
        var accountStore = new InMemoryUserAccountStore(new[] { admin, user });
        _authenticator = new CredentialAuthenticator(accountStore, passwordHasher);
    }

    [Test]
    public async Task AuthenticateAsync_RejectsIncorrectPassword()
    {
        var principal = await _authenticator.AuthenticateAsync("sam", "wrong-password");

        Assert.That(principal, Is.Null);
    }

    [Test]
    public async Task AuthenticateAsync_RejectsUnknownUsername()
    {
        var principal = await _authenticator.AuthenticateAsync("unknown", "User-password-123!");

        Assert.That(principal, Is.Null);
    }

    [Test]
    public async Task AuthenticateAsync_ReturnsAuthenticatedPrincipalForValidCredentials()
    {
        var principal = await _authenticator.AuthenticateAsync("sam", "User-password-123!");

        Assert.That(principal, Is.Not.Null);
        Assert.That(principal!.Identity!.IsAuthenticated, Is.True);
        Assert.That(principal.Identity.Name, Is.EqualTo("sam"));
    }

    [Test]
    public async Task AuthenticateAsync_AssignsRolesSoOnlyAdminHasAdminAccess()
    {
        var admin = await _authenticator.AuthenticateAsync("admin", "Admin-password-123!");
        var user = await _authenticator.AuthenticateAsync("sam", "User-password-123!");

        Assert.That(admin!.IsInRole("admin"), Is.True);
        Assert.That(user!.IsInRole("admin"), Is.False);
        Assert.That(user.IsInRole("user"), Is.True);
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
}
