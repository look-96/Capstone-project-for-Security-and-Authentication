namespace SafeVault.Core.Authentication;

/// <summary>Demo-only account store. Replace with a persistent store in production.</summary>
public sealed class InMemoryUserAccountStore : IUserAccountStore
{
    private readonly IReadOnlyDictionary<string, UserAccount> _accounts;

    public InMemoryUserAccountStore(IEnumerable<UserAccount> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        _accounts = accounts.ToDictionary(account => account.Username, StringComparer.OrdinalIgnoreCase);
    }

    public Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _accounts.TryGetValue(username, out var account);
        return Task.FromResult(account);
    }
}
