using System.Net.Mail;
using System.Text.RegularExpressions;

namespace SafeVault.Core;

public static partial class UserInputValidator
{
    private const int MaximumEmailLength = 254;

    public static UserInput Validate(string? username, string? email)
    {
        var normalizedUsername = username?.Trim();
        var normalizedEmail = email?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedUsername) || !UsernamePattern().IsMatch(normalizedUsername))
        {
            throw new ArgumentException("Username must be 3–32 characters and contain only letters, digits, '.', '_' or '-'.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(normalizedEmail) || normalizedEmail.Length > MaximumEmailLength || !IsValidEmail(normalizedEmail))
        {
            throw new ArgumentException("Enter a valid email address no longer than 254 characters.", nameof(email));
        }

        return new UserInput(normalizedUsername, normalizedEmail);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email);
            return string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]{3,32}$", RegexOptions.CultureInvariant)]
    private static partial Regex UsernamePattern();
}
