namespace SafeVault.Core.Authentication;

public sealed record UserAccount(string Username, string PasswordHash, string Role);
