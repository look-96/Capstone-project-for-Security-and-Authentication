using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace SafeVault.Core.Authentication;

public sealed class CredentialAuthenticator
{
    private readonly IUserAccountStore _accountStore;
    private readonly IPasswordHasher<UserAccount> _passwordHasher;
    private readonly UserAccount _dummyAccount;

    public CredentialAuthenticator(IUserAccountStore accountStore, IPasswordHasher<UserAccount> passwordHasher)
    {
        _accountStore = accountStore ?? throw new ArgumentNullException(nameof(accountStore));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));

        var dummyAccount = new UserAccount("__invalid__", string.Empty, "user");
        _dummyAccount = dummyAccount with
        {
            PasswordHash = _passwordHasher.HashPassword(
                dummyAccount,
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))
        };
    }

    public async Task<ClaimsPrincipal?> AuthenticateAsync(
        string? username,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var account = await _accountStore.FindByUsernameAsync(username.Trim(), cancellationToken);
        var accountForVerification = account ?? _dummyAccount;
        var passwordHash = account?.PasswordHash ?? _dummyAccount.PasswordHash;
        var verification = _passwordHasher.VerifyHashedPassword(accountForVerification, passwordHash, password);

        if (account is null || verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Username),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Role, account.Role)
        };
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
