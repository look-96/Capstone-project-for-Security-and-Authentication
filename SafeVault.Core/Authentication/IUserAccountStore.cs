namespace SafeVault.Core.Authentication;

public interface IUserAccountStore
{
    Task<UserAccount?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
